"use client"

import { PageHeader } from "@/components/shared/PageHeader"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Cable, Radio, ListTodo, Waves, Network } from "lucide-react"
import Link from "next/link"

export default function NetworkPage() {
  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="Network Inventory" />
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <Link href="/network/elements">
          <Card className="transition-colors hover:bg-muted/50 cursor-pointer">
            <CardHeader>
              <Cable className="h-8 w-8 mb-2 text-primary" />
              <CardTitle>Network Elements</CardTitle>
              <p className="text-sm text-muted-foreground">Routers, switches, OLTs, ONTs</p>
            </CardHeader>
          </Card>
        </Link>
        <Link href="/network/olts">
          <Card className="transition-colors hover:bg-muted/50 cursor-pointer">
            <CardHeader>
              <Radio className="h-8 w-8 mb-2 text-primary" />
              <CardTitle>OLTs</CardTitle>
              <p className="text-sm text-muted-foreground">Optical line terminals and PON ports</p>
            </CardHeader>
          </Card>
        </Link>
        <Link href="/network/subnets">
          <Card className="transition-colors hover:bg-muted/50 cursor-pointer">
            <CardHeader>
              <Network className="h-8 w-8 mb-2 text-primary" />
              <CardTitle>Subnets</CardTitle>
              <p className="text-sm text-muted-foreground">IP subnets and VLAN assignments</p>
            </CardHeader>
          </Card>
        </Link>
        <Link href="/network/vlans">
          <Card className="transition-colors hover:bg-muted/50 cursor-pointer">
            <CardHeader>
              <Waves className="h-8 w-8 mb-2 text-primary" />
              <CardTitle>VLANs</CardTitle>
              <p className="text-sm text-muted-foreground">Virtual LANs</p>
            </CardHeader>
          </Card>
        </Link>
      </div>
    </div>
  )
}
