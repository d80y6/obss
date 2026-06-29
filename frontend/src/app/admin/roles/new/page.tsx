"use client"

import Link from "next/link"
import { Button } from "@/components/ui/button"
import { Card, CardContent } from "@/components/ui/card"
import { PageHeader } from "@/components/shared/PageHeader"
import { ShieldBan } from "lucide-react"

export default function NewRolePage() {
  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="New Role" backHref="/admin/roles" />
      <Card>
        <CardContent className="pt-6 text-center py-12">
          <ShieldBan className="mx-auto h-12 w-12 text-muted-foreground mb-4" />
          <p className="text-muted-foreground mb-4">Role creation is not yet available via API.</p>
          <Button asChild variant="outline">
            <Link href="/admin/roles">Back to Roles</Link>
          </Button>
        </CardContent>
      </Card>
    </div>
  )
}
