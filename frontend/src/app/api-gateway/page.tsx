"use client"

import { PageHeader } from "@/components/shared/PageHeader"
import { Card, CardHeader, CardTitle } from "@/components/ui/card"
import { Waypoints, Key, Handshake } from "lucide-react"
import Link from "next/link"

export default function ApiGatewayPage() {
  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="API Gateway" />
      <div className="grid gap-4 md:grid-cols-3">
        <Link href="/api-gateway/routes">
          <Card className="transition-colors hover:bg-muted/50 cursor-pointer">
            <CardHeader>
              <Waypoints className="h-8 w-8 mb-2 text-primary" />
              <CardTitle>Routes</CardTitle>
              <p className="text-sm text-muted-foreground">Manage API routes and endpoints</p>
            </CardHeader>
          </Card>
        </Link>
        <Link href="/api-gateway/api-keys">
          <Card className="transition-colors hover:bg-muted/50 cursor-pointer">
            <CardHeader>
              <Key className="h-8 w-8 mb-2 text-primary" />
              <CardTitle>API Keys</CardTitle>
              <p className="text-sm text-muted-foreground">Manage API keys for partners</p>
            </CardHeader>
          </Card>
        </Link>
        <Link href="/api-gateway/partners">
          <Card className="transition-colors hover:bg-muted/50 cursor-pointer">
            <CardHeader>
              <Handshake className="h-8 w-8 mb-2 text-primary" />
              <CardTitle>Partners</CardTitle>
              <p className="text-sm text-muted-foreground">Manage integration partners</p>
            </CardHeader>
          </Card>
        </Link>
      </div>
    </div>
  )
}
