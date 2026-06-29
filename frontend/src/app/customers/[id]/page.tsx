"use client"

import { useParams, useRouter } from "next/navigation"
import { useState } from "react"
import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { EntityTabs } from "@/components/shared/EntityTabs"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { DataTable, Column } from "@/components/shared/DataTable"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { useCustomer } from "@/api/hooks/useCustomer"
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import api from "@/services/api"
import { queryKeys } from "@/lib/query-keys"
import { toast } from "@/components/ui/toast"
import type {
  ContactDto,
  NoteDto,
  OrderDto,
  SubscriptionDto,
  InvoiceDto,
  PaymentDto,
} from "@/api/generated"
import type { AuditEntryDto } from "@/types/api"

export default function CustomerDetailPage() {
  const params = useParams()
  const router = useRouter()
  const id = params.id as string
  const queryClient = useQueryClient()

  const [noteContent, setNoteContent] = useState("")
  const [noteCategory, setNoteCategory] = useState("general")

  const { data: customer, isLoading } = useCustomer(id)

  const { data: contacts } = useQuery({
    queryKey: queryKeys.customers.contacts(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/crm/customers/${id}/contacts`)
      return res.data as ContactDto[]
    },
    enabled: !!id,
  })

  const { data: notes } = useQuery({
    queryKey: queryKeys.customers.notes(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/crm/customers/${id}/notes`)
      return res.data as NoteDto[]
    },
    enabled: !!id,
  })

  const { data: orders } = useQuery({
    queryKey: queryKeys.orders.list({ customerId: id }),
    queryFn: async () => {
      const res = await api.get(`/api/v1/orders/customers/${id}/orders`)
      return res.data as OrderDto[]
    },
    enabled: !!id,
  })

  const { data: subscriptions } = useQuery({
    queryKey: queryKeys.subscriptions.list({ customerId: id }),
    queryFn: async () => {
      const res = await api.get(`/api/v1/subscriptions/customers/${id}/subscriptions`)
      return res.data as SubscriptionDto[]
    },
    enabled: !!id,
  })

  const { data: invoices } = useQuery({
    queryKey: queryKeys.invoices.list({ customerId: id }),
    queryFn: async () => {
      const res = await api.get(`/api/v1/invoices/invoices?customerId=${id}`)
      return res.data as InvoiceDto[]
    },
    enabled: !!id,
  })

  const { data: payments } = useQuery({
    queryKey: queryKeys.payments.list({ customerId: id }),
    queryFn: async () => {
      const res = await api.get(`/api/v1/payments/payments?customerId=${id}`)
      return res.data as PaymentDto[]
    },
    enabled: !!id,
  })

  const createNoteMutation = useMutation({
    mutationFn: async (data: { content: string; category: string }) => {
      const res = await api.post(`/api/v1/crm/customers/${id}/notes`, data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.customers.notes(id) })
      toast({ title: "Note added", description: "Note has been added successfully." })
      setNoteContent("")
      setNoteCategory("general")
    },
    onError: () => {
      toast({ title: "Error", description: "Failed to add note.", variant: "destructive" })
    },
  })

  const { data: auditEntries } = useQuery({
    queryKey: queryKeys.audit.entity("Customer", id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/audit/entities/Customer/${id}`)
      return res.data as AuditEntryDto[]
    },
    enabled: !!id,
  })

  const contactColumns: Column<ContactDto>[] = [
    { id: "firstName", header: "First Name", accessorKey: "firstName" },
    { id: "lastName", header: "Last Name", accessorKey: "lastName" },
    { id: "email", header: "Email", accessorKey: "email" },
    { id: "phone", header: "Phone", accessorKey: "phoneNumber" },
    { id: "role", header: "Position", accessorKey: "position" },
  ]

  const orderColumns: Column<OrderDto>[] = [
    { id: "orderNumber", header: "Order #", accessorKey: "orderNumber" },
    { id: "status", header: "Status", cell: (row) => <StatusBadge status={row.status} /> },
    { id: "grandTotal", header: "Total", cell: (row) => `${row.currency ?? ""} ${(row.grandTotal ?? 0).toLocaleString()}` },
    { id: "createdAt", header: "Date", cell: (row) => new Date(row.createdAt).toLocaleDateString() },
  ]

  const invoiceColumns: Column<InvoiceDto>[] = [
    { id: "invoiceNumber", header: "Invoice #", accessorKey: "invoiceNumber" },
    { id: "status", header: "Status", cell: (row) => <StatusBadge status={row.status} /> },
    { id: "totalAmount", header: "Amount", cell: (row) => `${row.currency ?? ""} ${(row.totalAmount ?? 0).toLocaleString()}` },
    { id: "dueDate", header: "Due Date", cell: (row) => new Date(row.dueDate).toLocaleDateString() },
  ]

  const tabs = [
    {
      id: "overview",
      label: "Overview",
      content: (
        <div className="space-y-6">
          <EntityMetadata
            title="Customer Details"
            loading={isLoading}
            fields={[
              { label: "Name", value: customer?.displayName ?? "-" },
              { label: "Email", value: customer?.email ?? "-" },
              { label: "Phone", value: customer?.phoneNumber ?? "-" },
              { label: "Status", value: customer ? <StatusBadge status={customer.status} /> : "-" },
              { label: "Type", value: customer?.customerType ?? "-" },
              { label: "Company", value: customer?.companyName ?? "-" },
              { label: "Tax ID", value: customer?.taxNumber ?? "-" },
              { label: "Country", value: customer?.countryCode ?? "-" },
              { label: "Credit Limit", value: customer ? `${customer.currency} ${customer.creditLimit?.toLocaleString()}` : "-" },
              { label: "Created", value: customer?.createdAt ? new Date(customer.createdAt).toLocaleDateString() : "-" },
            ]}
            columns={3}
          />
        </div>
      ),
    },
    {
      id: "contacts",
      label: `Contacts (${(contacts ?? []).length})`,
      content: (
        <Card>
          <CardHeader className="flex flex-row items-center justify-between">
            <CardTitle className="text-base">Contacts</CardTitle>
            <Button size="sm" onClick={() => router.push(`/customers/${id}/contacts/new`)}>
              Add Contact
            </Button>
          </CardHeader>
          <CardContent>
            <DataTable
              columns={contactColumns}
              data={contacts ?? []}
              emptyTitle="No contacts"
              rowKey={(row) => row.id}
              onRowClick={(row) => router.push(`/customers/${id}/contacts/${row.id}/edit`)}
            />
          </CardContent>
        </Card>
      ),
    },
    {
      id: "notes",
      label: `Notes (${(notes ?? []).length})`,
      content: (
        <div className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle className="text-base">Add Note</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                <div className="space-y-2">
                  <label className="text-sm font-medium">Category</label>
                  <select
                    className="flex h-9 w-full rounded-md border border-input bg-background px-3 py-1 text-sm shadow-sm"
                    value={noteCategory}
                    onChange={(e) => setNoteCategory(e.target.value)}
                  >
                    <option value="general">General</option>
                    <option value="support">Support</option>
                    <option value="billing">Billing</option>
                    <option value="technical">Technical</option>
                  </select>
                </div>
                <div className="space-y-2">
                  <label className="text-sm font-medium">Content</label>
                  <textarea
                    className="flex min-h-[80px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm shadow-sm"
                    value={noteContent}
                    onChange={(e) => setNoteContent(e.target.value)}
                    placeholder="Add a note..."
                  />
                </div>
                <Button
                  size="sm"
                  onClick={() => {
                    if (noteContent.trim()) {
                      createNoteMutation.mutate({ content: noteContent, category: noteCategory })
                    }
                  }}
                  disabled={createNoteMutation.isPending || !noteContent.trim()}
                >
                  {createNoteMutation.isPending ? "Adding..." : "Add Note"}
                </Button>
              </div>
            </CardContent>
          </Card>
          <Card>
            <CardHeader>
              <CardTitle className="text-base">Notes</CardTitle>
            </CardHeader>
            <CardContent>
              {!notes || notes.length === 0 ? (
                <p className="text-sm text-muted-foreground">No notes recorded.</p>
              ) : (
                <div className="space-y-4">
                  {notes.map((note) => (
                    <div key={note.id} className="border rounded-lg p-4">
                      <div className="flex items-center justify-between mb-2">
                        <span className="text-sm font-medium">{note.author}</span>
                        <span className="text-xs text-muted-foreground">
                          {new Date(note.createdAt).toLocaleString()}
                        </span>
                      </div>
                      <p className="text-sm text-muted-foreground">{note.content}</p>
                      {note.category && (
                        <span className="inline-block mt-2 text-xs bg-muted px-2 py-0.5 rounded">
                          {note.category}
                        </span>
                      )}
                    </div>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>
        </div>
      ),
    },
    {
      id: "orders",
      label: `Orders (${(orders ?? []).length})`,
      content: (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Orders</CardTitle>
          </CardHeader>
          <CardContent>
            <DataTable
              columns={orderColumns}
              data={orders ?? []}
              emptyTitle="No orders"
              rowKey={(row) => row.id}
            />
          </CardContent>
        </Card>
      ),
    },
    {
      id: "subscriptions",
      label: `Subscriptions (${(subscriptions ?? []).length})`,
      content: (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Subscriptions</CardTitle>
          </CardHeader>
          <CardContent>
            <DataTable
              columns={[
                { id: "id", header: "ID", cell: (row) => row.id.slice(0, 8) },
                { id: "offerName", header: "Offer", accessorKey: "offerName" },
                { id: "status", header: "Status", cell: (row) => <StatusBadge status={row.status} /> },
                { id: "price", header: "Amount", cell: (row) => `${row.currency ?? ""} ${(row.price ?? 0).toLocaleString()}` },
                { id: "startDate", header: "Start", cell: (row) => new Date(row.startDate).toLocaleDateString() },
              ]}
              data={subscriptions ?? []}
              emptyTitle="No subscriptions"
              rowKey={(row) => row.id}
            />
          </CardContent>
        </Card>
      ),
    },
    {
      id: "invoices",
      label: `Invoices (${(invoices ?? []).length})`,
      content: (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Invoices</CardTitle>
          </CardHeader>
          <CardContent>
            <DataTable
              columns={invoiceColumns}
              data={invoices ?? []}
              emptyTitle="No invoices"
              rowKey={(row) => row.id}
            />
          </CardContent>
        </Card>
      ),
    },
    {
      id: "payments",
      label: `Payments (${(payments ?? []).length})`,
      content: (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Payments</CardTitle>
          </CardHeader>
          <CardContent>
            <DataTable
              columns={[
                { id: "paymentNumber", header: "Payment #", accessorKey: "paymentNumber" },
                { id: "amount", header: "Amount", cell: (row) => `${row.currency ?? ""} ${(row.amount ?? 0).toLocaleString()}` },
                { id: "method", header: "Method", cell: (row) => (row.method ?? "").replace(/_/g, " ") },
                { id: "status", header: "Status", cell: (row) => <StatusBadge status={row.status} /> },
                { id: "paidAt", header: "Date", cell: (row) => new Date(row.paidAt || row.createdAt).toLocaleDateString() },
              ]}
              data={payments ?? []}
              emptyTitle="No payments"
              rowKey={(row) => row.id}
            />
          </CardContent>
        </Card>
      ),
    },
    {
      id: "audit",
      label: "Audit",
      content: (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Audit Trail</CardTitle>
          </CardHeader>
          <CardContent>
            <DataTable
              columns={[
                { id: "action", header: "Action", accessorKey: "action" },
                { id: "performedByName", header: "Actor", accessorKey: "performedByName" },
                { id: "performedAt", header: "Timestamp", cell: (row) => row.performedAt ? new Date(row.performedAt).toLocaleString() : "-" },
              ]}
              data={auditEntries ?? []}
              emptyTitle="No audit entries"
              rowKey={(row) => row.id}
            />
          </CardContent>
        </Card>
      ),
    },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <EntityHeader
        title={customer ? customer.displayName : "Customer"}
        subtitle={customer?.email}
        status={customer?.status}
        backHref="/customers"
        editHref={`/customers/${id}/edit`}
        loading={isLoading}
      />

      <EntityTabs tabs={tabs} defaultTab="overview" />
    </div>
  )
}
