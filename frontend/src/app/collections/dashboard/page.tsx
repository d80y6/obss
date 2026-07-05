"use client"

import { useCollectionDashboard } from "@/api/hooks/use-collections"
import { PageHeader } from "@/components/shared/PageHeader"
import { MetricCard } from "@/components/shared/MetricCard"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { useQuery } from "@tanstack/react-query"
import api from "@/services/api"
import { Landmark, FileText, Handshake, Mail, DollarSign } from "lucide-react"
import type { CollectionCaseDto } from "@/api/generated"

export default function CollectionsDashboardPage() {
  const { data: dashboard, isLoading } = useCollectionDashboard()

  const { data: recentCases } = useQuery({
    queryKey: ["collections-recent-cases"],
    queryFn: async () => {
      const res = await api.get("/api/v1/collections/cases")
      const cases = (res.data ?? []) as CollectionCaseDto[]
      return cases.slice(0, 5)
    },
  })

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="Collections Dashboard" />
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <MetricCard
          title="Open Cases"
          value={dashboard?.openCases ?? 0}
          icon={FileText}
          loading={isLoading}
        />
        <MetricCard
          title="Active Arrangements"
          value={dashboard?.activeArrangements ?? 0}
          icon={Handshake}
          loading={isLoading}
        />
        <MetricCard
          title="Dunning Notices Sent"
          value={dashboard?.dunningNoticesSent ?? 0}
          icon={Mail}
          loading={isLoading}
        />
        <MetricCard
          title="Total Overdue ($)"
          value={`$${(dashboard?.totalOverdue ?? 0).toLocaleString()}`}
          icon={DollarSign}
          loading={isLoading}
        />
      </div>
      <Card>
        <CardHeader>
          <CardTitle className="text-base">Recent Activity</CardTitle>
        </CardHeader>
        <CardContent>
          {(!recentCases || recentCases.length === 0) ? (
            <p className="text-sm text-muted-foreground">No recent cases.</p>
          ) : (
            <div className="space-y-3">
              {recentCases.map((c) => (
                <div key={c.id} className="flex items-center justify-between border-b py-2">
                  <div className="flex items-center gap-3">
                    <Landmark className="h-4 w-4 text-muted-foreground" />
                    <span className="text-sm font-medium">{c.customerName}</span>
                  </div>
                  <div className="flex items-center gap-3">
                    <StatusBadge status={c.status} />
                    <span className="text-sm text-muted-foreground">
                      ${(c.totalOverdueAmount ?? 0).toLocaleString()}
                    </span>
                    <span className="text-xs text-muted-foreground">
                      {c.lastActionAt ? new Date(c.lastActionAt).toLocaleDateString() : ""}
                    </span>
                  </div>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  )
}
