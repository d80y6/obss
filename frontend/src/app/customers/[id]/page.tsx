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
import { useCustomerContacts } from "@/api/hooks/useCustomerContacts"
import { useCustomerNotes } from "@/api/hooks/useCustomerNotes"
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import api from "@/services/api"
import { queryKeys } from "@/lib/query-keys"
import { toast } from "@/components/ui/toast"
import { useAuditLog } from "@/api/hooks/useAuditLog"
import { Trash2, ShieldCheck } from "lucide-react"
import type {
  ContactDto,
  OrderDto,
  SubscriptionDto,
  InvoiceDto,
  PaymentDto,
  IdentityDocumentDto,
  CreditProfileDto,
} from "@/api/generated"

export default function CustomerDetailPage() {
  const params = useParams()
  const router = useRouter()
  const id = params.id as string
  const queryClient = useQueryClient()

  const [noteContent, setNoteContent] = useState("")
  const [noteCategory, setNoteCategory] = useState("general")

  const { data: customer, isLoading } = useCustomer(id)

  const { data: contacts } = useCustomerContacts(id)
  const { data: notes } = useCustomerNotes(id)

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

  const { data: creditProfiles } = useQuery({
    queryKey: ["customer", id, "credit-profiles"],
    queryFn: async () => {
      const res = await api.get(`/api/v1/crm/customers/${id}/credit-profiles`)
      return res.data as CreditProfileDto[]
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

  const deleteMutation = useMutation({
    mutationFn: async () => {
      await api.delete(`/api/v1/crm/customers/${id}`)
    },
    onSuccess: () => {
      toast({ title: "Customer deleted", description: "Customer has been deleted." })
      router.push("/customers")
    },
    onError: () => {
      toast({ title: "Error", description: "Failed to delete customer.", variant: "destructive" })
    },
  })

  const deleteContactMutation = useMutation({
    mutationFn: async (contactId: string) => {
      await api.delete(`/api/v1/crm/customers/${id}/contacts/${contactId}`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.customers.contacts(id) })
      toast({ title: "Contact deleted", description: "Contact has been removed." })
    },
    onError: () => {
      toast({ title: "Error", description: "Failed to delete contact.", variant: "destructive" })
    },
  })

  const kycVerifyMutation = useMutation({
    mutationFn: async () => {
      await api.post(`/api/v1/crm/customers/${id}/kyc-verify`, { customerId: id })
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["customer", id] })
      toast({ title: "KYC Verified", description: "Customer identity has been verified." })
    },
    onError: () => {
      toast({ title: "Error", description: "KYC verification failed.", variant: "destructive" })
    },
  })

  const { data: auditEntries } = useAuditLog("Customer", id)

  const contactColumns: Column<ContactDto>[] = [
    { id: "firstName", header: "First Name", accessorKey: "firstName" },
    { id: "lastName", header: "Last Name", accessorKey: "lastName" },
    { id: "email", header: "Email", accessorKey: "email" },
    { id: "phone", header: "Phone", accessorKey: "phoneNumber" },
    { id: "role", header: "Position", accessorKey: "position" },
    {
      id: "actions",
      header: "",
      cell: (row) => (
        <Button
          variant="ghost"
          size="icon"
          className="h-8 w-8"
          onClick={(e) => {
            e.stopPropagation()
            deleteContactMutation.mutate(row.id)
          }}
          disabled={deleteContactMutation.isPending}
        >
          <Trash2 className="h-4 w-4 text-destructive" />
        </Button>
      ),
    },
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

  const characteristicColumns: Column<{ key: string; value: string; valueType: string }>[] = [
    { id: "key", header: "Key", accessorKey: "key" },
    { id: "value", header: "Value", accessorKey: "value" },
    { id: "valueType", header: "Type", accessorKey: "valueType" },
  ]

  const documentColumns: Column<IdentityDocumentDto>[] = [
    { id: "documentType", header: "Type", accessorKey: "documentType" },
    { id: "documentNumber", header: "Document #", accessorKey: "documentNumber" },
    { id: "issuingAuthority", header: "Authority", accessorKey: "issuingAuthority" },
    { id: "expiryDate", header: "Expiry", cell: (row) => row.expiryDate ? new Date(row.expiryDate).toLocaleDateString() : "-" },
    { id: "isVerified", header: "Verified", cell: (row) => row.isVerified ? "Yes" : "No" },
  ]

  const creditProfileColumns: Column<CreditProfileDto>[] = [
    { id: "score", header: "Score", accessorKey: "score" },
    { id: "scoreType", header: "Score Type", accessorKey: "scoreType" },
    { id: "validFrom", header: "Valid From", cell: (row) => row.validFrom ? new Date(row.validFrom).toLocaleDateString() : "-" },
    { id: "validUntil", header: "Valid Until", cell: (row) => row.validUntil ? new Date(row.validUntil).toLocaleDateString() : "-" },
    { id: "riskRating", header: "Risk Rating", accessorKey: "riskRating" },
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
              { label: "Description", value: customer?.description ?? "-" },
              { label: "Status Reason", value: customer?.statusReason ?? "-" },
              { label: "External ID", value: customer?.externalId ?? "-" },
              { label: "Valid From", value: customer?.validFrom ? new Date(customer.validFrom).toLocaleDateString() : "-" },
              { label: "Valid Until", value: customer?.validUntil ? new Date(customer.validUntil).toLocaleDateString() : "-" },
              { label: "Created", value: customer?.createdAt ? new Date(customer.createdAt).toLocaleDateString() : "-" },
            ]}
            columns={3}
          />

          {customer?.individual && (
            <Card>
              <CardHeader className="flex flex-row items-center justify-between">
                <CardTitle className="text-base">Individual (Engaged Party)</CardTitle>
                <div className="flex gap-2">
                  <Button
                    size="sm"
                    variant="outline"
                    onClick={() => kycVerifyMutation.mutate()}
                    disabled={kycVerifyMutation.isPending || customer.individual.kycStatus === "Verified"}
                  >
                    <ShieldCheck className="h-4 w-4 mr-1" />
                    {kycVerifyMutation.isPending ? "Verifying..." : "Verify KYC"}
                  </Button>
                  <Button size="sm" variant="outline" onClick={() => router.push(`/customers/${id}/edit`)}>
                    Edit
                  </Button>
                </div>
              </CardHeader>
              <CardContent>
                <EntityMetadata
                  title=""
                  loading={false}
                  fields={[
                    { label: "Name", value: `${customer.individual.firstName} ${customer.individual.lastName}` },
                    { label: "Middle Name", value: customer.individual.middleName ?? "-" },
                    { label: "Salutation", value: customer.individual.salutation ?? "-" },
                    { label: "Title", value: customer.individual.title ?? "-" },
                    { label: "Birth Date", value: customer.individual.birthDate ? new Date(customer.individual.birthDate).toLocaleDateString() : "-" },
                    { label: "Nationality", value: customer.individual.nationality ?? "-" },
                    { label: "Gender", value: customer.individual.gender ?? "-" },
                    { label: "KYC Status", value: <StatusBadge status={customer.individual.kycStatus} /> },
                    { label: "KYC Verified At", value: customer.individual.kycVerifiedAt ? new Date(customer.individual.kycVerifiedAt).toLocaleString() : "-" },
                    { label: "Risk Rating", value: customer.individual.riskRating },
                  ]}
                  columns={2}
                />
                {customer.individual.documents.length > 0 && (
                  <div className="mt-4">
                    <DataTable
                      columns={documentColumns}
                      data={customer.individual.documents}
                      emptyTitle="No identity documents"
                      rowKey={(row) => row.id}
                    />
                  </div>
                )}
              </CardContent>
            </Card>
          )}

          {customer?.organization && (
            <Card>
              <CardHeader className="flex flex-row items-center justify-between">
                <CardTitle className="text-base">Organization (Engaged Party)</CardTitle>
                <Button size="sm" variant="outline" onClick={() => router.push(`/customers/${id}/edit`)}>
                  Edit
                </Button>
              </CardHeader>
              <CardContent>
                <EntityMetadata
                  title=""
                  loading={false}
                  fields={[
                    { label: "Trading Name", value: customer.organization.tradingName },
                    { label: "Company Type", value: customer.organization.companyType },
                    { label: "Industry", value: customer.organization.industry ?? "-" },
                    { label: "Registration Number", value: customer.organization.registrationNumber ?? "-" },
                    { label: "Tax Number", value: customer.organization.taxNumber ?? "-" },
                    { label: "Country of Registration", value: customer.organization.countryOfRegistration ?? "-" },
                    { label: "KYC Status", value: <StatusBadge status={customer.organization.kycStatus} /> },
                    { label: "KYC Verified At", value: customer.organization.kycVerifiedAt ? new Date(customer.organization.kycVerifiedAt).toLocaleString() : "-" },
                    { label: "Risk Rating", value: customer.organization.kycStatus },
                  ]}
                  columns={2}
                />
              </CardContent>
            </Card>
          )}

          {customer && customer.characteristics.length > 0 && (
            <Card>
              <CardHeader>
                <CardTitle className="text-base">Characteristics</CardTitle>
              </CardHeader>
              <CardContent>
                <DataTable
                  columns={characteristicColumns}
                  data={customer.characteristics}
                  emptyTitle="No characteristics"
                  rowKey={(row) => row.key}
                />
              </CardContent>
            </Card>
          )}

          {customer && customer.relatedParties.length > 0 && (
            <Card>
              <CardHeader>
                <CardTitle className="text-base">Related Parties</CardTitle>
              </CardHeader>
              <CardContent>
                <DataTable
                  columns={[
                    { id: "name", header: "Name", accessorKey: "name" },
                    { id: "role", header: "Role", accessorKey: "role" },
                    { id: "referredId", header: "Referred ID", cell: (row) => row.referredId.slice(0, 8) },
                    { id: "referredType", header: "Referred Type", accessorKey: "referredType" },
                  ]}
                  data={customer.relatedParties}
                  emptyTitle="No related parties"
                  rowKey={(row) => row.referredId}
                />
              </CardContent>
            </Card>
          )}
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
      id: "credit-profiles",
      label: `Credit Profiles (${(creditProfiles ?? []).length})`,
      content: (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Credit Profile History</CardTitle>
          </CardHeader>
          <CardContent>
            <DataTable
              columns={creditProfileColumns}
              data={creditProfiles ?? []}
              emptyTitle="No credit profiles"
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
        onDelete={() => {
          if (window.confirm("Are you sure you want to delete this customer? This action cannot be undone.")) {
            deleteMutation.mutate()
          }
        }}
        loading={isLoading}
      />

      <EntityTabs tabs={tabs} defaultTab="overview" />
    </div>
  )
}
