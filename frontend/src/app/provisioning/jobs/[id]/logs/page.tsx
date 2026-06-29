"use client"

import { useParams } from "next/navigation"
import { PageHeader } from "@/components/shared/PageHeader"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { useProvisioningJobLogs } from "@/api/hooks/use-provisioning-jobs"
import { AlertCircle, CheckCircle, Info, Loader2 } from "lucide-react"
import { cn } from "@/lib/utils"

export default function ProvisioningJobLogsPage() {
  const params = useParams()
  const id = params.id as string

  const { data: logs, isLoading } = useProvisioningJobLogs(id)

  const levelIcon = (level: string) => {
    switch (level) {
      case "error": return <AlertCircle className="h-4 w-4 text-destructive" />
      case "success": return <CheckCircle className="h-4 w-4 text-green-500" />
      case "info": return <Info className="h-4 w-4 text-blue-500" />
      default: return <Loader2 className="h-4 w-4 text-muted-foreground" />
    }
  }

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="Job Logs" backHref={`/provisioning/jobs/${id}`} />
      <Card>
        <CardHeader><CardTitle className="text-base">Execution Log</CardTitle></CardHeader>
        <CardContent>
          {isLoading ? (
            <div className="flex items-center justify-center py-8">
              <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
            </div>
          ) : (!logs || logs.length === 0) ? (
            <p className="text-sm text-muted-foreground">No log entries.</p>
          ) : (
            <div className="space-y-2">
              {logs.map((entry) => (
                <div key={entry.id} className={cn(
                  "border rounded-md p-3",
                  entry.level === "error" && "border-destructive/20 bg-destructive/5",
                  entry.level === "success" && "border-green-200 bg-green-50 dark:border-green-800 dark:bg-green-950",
                )}>
                  <div className="flex items-start gap-2">
                    <div className="mt-0.5">{levelIcon(entry.level)}</div>
                    <div className="flex-1 min-w-0">
                      <div className="flex items-center gap-2">
                        <span className="text-xs font-medium">Step {entry.step}</span>
                        <StatusBadge status={entry.level} />
                        <span className="text-xs text-muted-foreground ml-auto">
                          {new Date(entry.timestamp).toLocaleString()}
                        </span>
                      </div>
                      <p className="text-sm mt-1">{entry.message}</p>
                      {entry.result && (
                        <p className="text-xs text-muted-foreground mt-1">Result: {entry.result}</p>
                      )}
                      {entry.error && (
                        <p className="text-xs text-destructive mt-1">Error: {entry.error}</p>
                      )}
                    </div>
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
