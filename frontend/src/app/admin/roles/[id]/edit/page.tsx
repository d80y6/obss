"use client"

import { useParams } from "next/navigation"
import Link from "next/link"
import { Button } from "@/components/ui/button"
import { Card, CardContent } from "@/components/ui/card"
import { PageHeader } from "@/components/shared/PageHeader"
import { ShieldBan } from "lucide-react"

export default function EditRolePage() {
  const params = useParams()
  const id = params.id as string

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="Edit Role" backHref={`/admin/roles/${id}`} />
      <Card>
        <CardContent className="pt-6 text-center py-12">
          <ShieldBan className="mx-auto h-12 w-12 text-muted-foreground mb-4" />
          <p className="text-muted-foreground mb-4">Role editing is not yet available via API.</p>
          <Button asChild variant="outline">
            <Link href={`/admin/roles/${id}`}>Back to Role</Link>
          </Button>
        </CardContent>
      </Card>
    </div>
  )
}
