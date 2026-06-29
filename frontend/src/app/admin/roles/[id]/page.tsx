"use client"

import { useParams } from "next/navigation"
import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { EntityTabs } from "@/components/shared/EntityTabs"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import { useRole } from "@/api/hooks/useRole"
import { useQuery } from "@tanstack/react-query"
import api from "@/services/api"
import { queryKeys } from "@/lib/query-keys"
import { IamUserDto, AuditEntryDto } from "@/types/api"

export default function RoleDetailPage() {
  const params = useParams()
  const id = params.id as string

  const { data: role, isLoading } = useRole(id)

  const { data: users, error: usersError } = useQuery({
    queryKey: queryKeys.users.list(),
    queryFn: async () => {
      const res = await api.get("/api/v1/iam/users")
      return res.data as IamUserDto[]
    },
  })

  const { data: auditEntries, error: auditError } = useQuery({
    queryKey: queryKeys.audit.entity("Role", id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/audit/entities/Role/${id}`)
      return res.data as AuditEntryDto[]
    },
    enabled: !!id,
  })

  const usersWithRole = (users ?? []).filter((u) => u.role === role?.name)

  const tabs = [
    {
      id: "overview",
      label: "Overview",
      content: (
        <EntityMetadata
          title="Role Details"
          loading={isLoading}
          fields={[
            { label: "Name", value: role?.name ?? "-" },
            { label: "Description", value: role?.description ?? "-" },
            { label: "Permissions", value: (role?.permissions ?? []).map((p) => p.name).join(", ") || "-" },
            { label: "Assigned Users", value: String(usersWithRole.length) },
            { label: "Created", value: role?.createdAt ? new Date(role.createdAt).toLocaleDateString() : "-" },
            { label: "Updated", value: role?.updatedAt ? new Date(role.updatedAt).toLocaleDateString() : "-" },
          ]}
        />
      ),
    },
    {
      id: "users",
      label: `Users (${usersWithRole.length})`,
      content: (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Users with this Role</CardTitle>
          </CardHeader>
          <CardContent>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Username</TableHead>
                  <TableHead>Email</TableHead>
                  <TableHead>Name</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {usersWithRole.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={3} className="text-center text-muted-foreground">
                      No users assigned this role.
                    </TableCell>
                  </TableRow>
                ) : (
                  usersWithRole.map((user) => (
                    <TableRow key={user.id}>
                      <TableCell className="font-medium">{user.username}</TableCell>
                      <TableCell>{user.email}</TableCell>
                      <TableCell>{user.firstName} {user.lastName}</TableCell>
                    </TableRow>
                  ))
                )}
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
                </TableRow>
              </TableHeader>
              <TableBody>
                {(auditEntries ?? []).length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={3} className="text-center text-muted-foreground">
                      No audit entries found.
                    </TableCell>
                  </TableRow>
                ) : (
                  (auditEntries ?? []).map((entry) => (
                    <TableRow key={entry.id}>
                      <TableCell className="font-medium">{entry.action}</TableCell>
                      <TableCell>{entry.performedByName}</TableCell>
                      <TableCell>{new Date(entry.performedAt).toLocaleString()}</TableCell>
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
        title={role?.name ?? "Role"}
        subtitle={role?.description ?? undefined}
        backHref="/admin/roles"
        loading={isLoading}
      />

      <EntityTabs tabs={tabs} defaultTab="overview" />
    </div>
  )
}
