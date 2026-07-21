"use client"

import { useQuery } from "@tanstack/react-query"
import { Card, CardContent } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Skeleton } from "@/components/ui/skeleton"
import { FileText, Download } from "lucide-react"

async function fetchBills() {
  const res = await fetch("/api/v1/customers/me/bills")
  if (!res.ok) throw new Error("Failed to load bills")
  return res.json()
}

export default function PortalBillsPage() {
  const { data: bills, isLoading } = useQuery({
    queryKey: ["portal-bills"],
    queryFn: fetchBills,
  })

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">Bills & Payments</h1>
          <p className="text-muted-foreground">View and pay your bills</p>
        </div>
        <Button>Make a Payment</Button>
      </div>

      {isLoading ? (
        <div className="space-y-3">
          {[1, 2, 3].map((i) => (
            <Card key={i}><CardContent className="p-4"><Skeleton className="h-5 w-48" /></CardContent></Card>
          ))}
        </div>
      ) : (
        <div className="space-y-3">
          {bills?.length > 0 ? bills.map((bill: any) => (
            <Card key={bill.id}>
              <CardContent className="flex items-center justify-between p-4">
                <div className="flex items-center gap-3">
                  <FileText className="h-5 w-5 text-muted-foreground" />
                  <div>
                    <p className="font-medium">{bill.period}</p>
                    <p className="text-xs text-muted-foreground">Due: {bill.dueDate}</p>
                  </div>
                </div>
                <div className="flex items-center gap-4">
                  <span className={`font-semibold ${bill.status === "Paid" ? "text-green-600" : "text-destructive"}`}>
                    ${bill.amount}
                  </span>
                  <span className={`text-xs px-2 py-0.5 rounded ${
                    bill.status === "Paid" ? "bg-green-100 text-green-700" : "bg-yellow-100 text-yellow-700"
                  }`}>{bill.status}</span>
                  <Button variant="ghost" size="icon"><Download className="h-4 w-4" /></Button>
                </div>
              </CardContent>
            </Card>
          )) : (
            <p className="text-sm text-muted-foreground">No bills found</p>
          )}
        </div>
      )}
    </div>
  )
}
