"use client"

import { useQuery } from "@tanstack/react-query"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { MetricCard } from "@/components/shared/MetricCard"
import { PageHeader } from "@/components/shared/PageHeader"
import { LoadingState } from "@/components/shared/LoadingState"
import { ErrorFallback } from "@/components/shared/ErrorFallback"
import api from "@/services/api"
import { OrderDto, TicketDto, InvoiceDto, CustomerDto } from "@/types/api"
import { Users, ShoppingCart, Ticket, FileText, Plus } from "lucide-react"
import Link from "next/link"
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
} from "recharts"

export default function DashboardPage() {
  const { data: customers, isLoading: customersLoading } = useQuery({
    queryKey: ["dashboard-customers"],
    queryFn: async () => {
      const res = await api.get("/api/v1/crm/customers?pageSize=1")
      return res.data as CustomerDto[]
    },
  })

  const { data: orders, isLoading: ordersLoading } = useQuery({
    queryKey: ["dashboard-orders"],
    queryFn: async () => {
      const res = await api.get("/api/v1/orders/orders?pageSize=1")
      return res.data as OrderDto[]
    },
  })

  const { data: tickets, isLoading: ticketsLoading } = useQuery({
    queryKey: ["dashboard-tickets"],
    queryFn: async () => {
      const res = await api.get("/api/v1/ticketing/tickets?pageSize=1")
      return res.data as TicketDto[]
    },
  })

  const { data: invoices, isLoading: invoicesLoading } = useQuery({
    queryKey: ["dashboard-invoices"],
    queryFn: async () => {
      const res = await api.get("/api/v1/invoices/invoices?pageSize=1")
      return res.data as InvoiceDto[]
    },
  })

  const recentOrders = useQuery({
    queryKey: ["dashboard-recent-orders"],
    queryFn: async () => {
      const res = await api.get("/api/v1/orders/orders?limit=10&sort=createdAt,desc")
      return res.data as OrderDto[]
    },
  })

  const revenueQuery = useQuery({
    queryKey: ["dashboard-revenue"],
    queryFn: async () => {
      const res = await api.get("/api/v1/invoices/invoices/summary")
      return res.data as { dailyRevenue?: { date: string; revenue: number }[] }
    },
  })

  const statCards = [
    {
      title: "Total Customers",
      value: Array.isArray(customers) ? customers.length : 0,
      icon: Users,
      href: "/customers",
      loading: customersLoading,
    },
    {
      title: "Active Orders",
      value: Array.isArray(orders) ? orders.length : 0,
      icon: ShoppingCart,
      href: "/orders",
      loading: ordersLoading,
    },
    {
      title: "Open Tickets",
      value: Array.isArray(tickets) ? tickets.length : 0,
      icon: Ticket,
      href: "/tickets",
      loading: ticketsLoading,
    },
    {
      title: "Invoices",
      value: Array.isArray(invoices) ? invoices.length : 0,
      icon: FileText,
      href: "/invoices",
      loading: invoicesLoading,
    },
  ]

  const revenueData = revenueQuery.data?.dailyRevenue || []

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader
        title="Dashboard"
        actions={
          <>
            <Button asChild variant="outline" size="sm">
              <Link href="/customers/new">
                <Plus className="mr-1 h-4 w-4" /> New Customer
              </Link>
            </Button>
            <Button asChild variant="outline" size="sm">
              <Link href="/orders/new">
                <Plus className="mr-1 h-4 w-4" /> New Order
              </Link>
            </Button>
          </>
        }
      />

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        {statCards.map((card) => (
          <MetricCard
            key={card.title}
            title={card.title}
            value={card.value}
            icon={card.icon}
            href={card.href}
            loading={card.loading}
          />
        ))}
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Revenue</CardTitle>
          </CardHeader>
          <CardContent>
            {revenueQuery.isLoading ? (
              <LoadingState rows={3} />
            ) : revenueQuery.error ? (
              <ErrorFallback message="Failed to load revenue data" />
            ) : (
              <div className="h-80">
                <ResponsiveContainer width="100%" height="100%">
                  <LineChart data={revenueData}>
                    <CartesianGrid strokeDasharray="3 3" className="stroke-muted" />
                    <XAxis
                      dataKey="date"
                      tick={{ fontSize: 12 }}
                      className="text-muted-foreground"
                    />
                    <YAxis
                      tick={{ fontSize: 12 }}
                      className="text-muted-foreground"
                      tickFormatter={(v: number) => `$${v}`}
                    />
                    <Tooltip
                      contentStyle={{
                        backgroundColor: "hsl(var(--background))",
                        border: "1px solid hsl(var(--border))",
                        borderRadius: "var(--radius)",
                      }}
                      formatter={(value) => [`$${Number(value).toLocaleString()}`, "Revenue"]}
                    />
                    <Line
                      type="monotone"
                      dataKey="revenue"
                      stroke="hsl(var(--primary))"
                      strokeWidth={2}
                      dot={false}
                    />
                  </LineChart>
                </ResponsiveContainer>
              </div>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="text-base">Recent Orders</CardTitle>
          </CardHeader>
          <CardContent>
            {recentOrders.isLoading ? (
              <LoadingState rows={5} />
            ) : recentOrders.error ? (
              <ErrorFallback message="Failed to load orders" />
            ) : (
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Order #</TableHead>
                    <TableHead>Customer</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead className="text-right">Total</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {(recentOrders.data ?? []).slice(0, 10).map((order) => (
                    <TableRow key={order.id}>
                      <TableCell className="font-medium">{order.orderNumber}</TableCell>
                      <TableCell>{order.customerName}</TableCell>
                      <TableCell>
                        <StatusBadge status={order.status} />
                      </TableCell>
                      <TableCell className="text-right">
                        {order.currency} {(order.subTotal ?? 0).toLocaleString()}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  )
}
