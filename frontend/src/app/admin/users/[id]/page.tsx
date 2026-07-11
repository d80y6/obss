"use client"

import { useParams } from "next/navigation"
import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { EntityTabs } from "@/components/shared/EntityTabs"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import { useQuery } from "@tanstack/react-query"
import api from "@/services/api"
import { queryKeys } from "@/lib/query-keys"
import { useAuditLog } from "@/api/hooks/useAuditLog"
import type { IamUserDto, RoleDto } from "@/api/generated"

export default function UserDetailPage() {
  const params = useParams()
  const id = params.id as string

  const { data: user, isLoading } = useQuery({
    queryKey: queryKeys.users.detail(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/iam/users/${id}`)
      return res.data as IamUserDto
    },
    enabled: !!id,
  })

  const { data: roles } = useQuery({
    queryKey: queryKeys.roles.list(),
    queryFn: async () => {
      const res = await api.get("/api/v1/iam/roles")
      return res.data as RoleDto[]
    },
  })

  const { data: auditEntries } = useAuditLog("User", id)

  const tabs = [
    {
      id: "overview",
      label: "Overview",
      content: (
        <EntityMetadata
          title="User Details"
          loading={isLoading}
          fields={[
            { label: "Username", value: user?.username ?? "-" },
            { label: "Email", value: user?.email ?? "-" },
            { label: "First Name", value: user?.firstName ?? "-" },
            { label: "Last Name", value: user?.lastName ?? "-" },
            { label: "Role", value: user?.role ?? "-" },
            { label: "Status", value: user ? <StatusBadge status={user.isActive ? "ACTIVE" : "INACTIVE"} /> : "-" },
            { label: "Created", value: user?.createdAt ? new Date(user.createdAt).toLocaleDateString() : "-" },
            { label: "Updated", value: user?.updatedAt ? new Date(user.updatedAt).toLocaleDateString() : "-" },
          ]}
        />
      ),
    },
    {
      id: "roles",
      label: "Roles",
      content: (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Assigned Roles</CardTitle>
          </CardHeader>
          <CardContent>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Role</TableHead>
                  <TableHead>Description</TableHead>
                  <TableHead>Permissions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {(Array.isArray(roles) ? roles : [])
                  .filter((r) => r.name === user?.role)
                  .map((role) => (
                    <TableRow key={role.id}>
                      <TableCell className="font-medium">{role.name}</TableCell>
                      <TableCell>{role.description}</TableCell>
                      <TableCell>{(role.permissions ?? []).join(", ")}</TableCell>
                    </TableRow>
                  ))}
              </TableBody>
            </Table>
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
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Action</TableHead>
                  <TableHead>Actor</TableHead>
                  <TableHead>Timestamp</TableHead>
                  <TableHead>Details</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {(auditEntries ?? []).length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={4} className="text-center text-muted-foreground">
                      No audit entries found.
                    </TableCell>
                  </TableRow>
                ) : (
                  (auditEntries ?? []).map((entry) => (
                    <TableRow key={entry.id}>
                      <TableCell className="font-medium">{entry.action}</TableCell>
                      <TableCell>{entry.performedByName}</TableCell>
                      <TableCell>{new Date(entry.performedAt).toLocaleString()}</TableCell>
                      <TableCell className="max-w-xs truncate">
                        {JSON.stringify(entry.changes)}
                      </TableCell>
                    </TableRow>
                  ))
                )}
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      ),
    },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <EntityHeader
        title={user ? `${user.firstName} ${user.lastName}` : "User"}
        subtitle={user?.email}
        status={user?.isActive ? "ACTIVE" : "INACTIVE"}
        backHref="/admin"
        editHref={`/admin/users/${id}/edit`}
        loading={isLoading}
      />

      <EntityTabs tabs={tabs} defaultTab="overview" />
    </div>
  )
}
