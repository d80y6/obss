"use client"

import { useParams } from "next/navigation"
import { useRouter } from "next/navigation"
import { useState } from "react"
import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { EntityTabs } from "@/components/shared/EntityTabs"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { DataTable } from "@/components/shared/DataTable"
import type { Column } from "@/components/shared/DataTable"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Textarea } from "@/components/ui/textarea"
import { Checkbox } from "@/components/ui/checkbox"
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog"
import { Plus, Trash2, ChevronDown, ChevronRight } from "lucide-react"
import { useProductSpecification } from "@/api/hooks/useProductSpecification"
import { useDeleteProductSpecification } from "@/api/hooks/useDeleteProductSpecification"
import { useAuditLog } from "@/api/hooks/useAuditLog"
import { useCreateCharacteristic } from "@/api/hooks/useCreateCharacteristic"
import { useDeleteCharacteristic } from "@/api/hooks/useDeleteCharacteristic"
import { useCreateCharacteristicValue } from "@/api/hooks/useCreateCharacteristicValue"
import { useDeleteCharacteristicValue } from "@/api/hooks/useDeleteCharacteristicValue"
import { useCreateSpecificationRelationship } from "@/api/hooks/useCreateSpecificationRelationship"
import { useDeleteSpecificationRelationship } from "@/api/hooks/useDeleteSpecificationRelationship"
import { toast } from "@/components/ui/toast"
import { queryKeys } from "@/lib/query-keys"
import { useQueryClient } from "@tanstack/react-query"
import type {
  ProductSpecificationCharacteristicDto,
  ProductSpecificationRelationshipDto,
} from "@/api/generated/dto"

export default function ProductSpecificationDetailPage() {
  const params = useParams()
  const router = useRouter()
  const queryClient = useQueryClient()
  const id = params.id as string

  const { data: spec, isLoading } = useProductSpecification(id)
  const deleteSpec = useDeleteProductSpecification()
  const { data: auditEntries } = useAuditLog("ProductSpecification", id)

  const [charDialogOpen, setCharDialogOpen] = useState(false)
  const [charForm, setCharForm] = useState({
    name: "",
    description: "",
    valueType: "",
    configurable: false,
    isRequired: false,
    sortOrder: 0,
    minValue: "",
    maxValue: "",
  })
  const createChar = useCreateCharacteristic()
  const deleteChar = useDeleteCharacteristic()

  const [expandedCharId, setExpandedCharId] = useState<string | null>(null)
  const [valueDialogOpen, setValueDialogOpen] = useState(false)
  const [valueForm, setValueForm] = useState({
    value: "",
    unitOfMeasure: "",
    isDefault: false,
  })
  const createCharValue = useCreateCharacteristicValue()
  const deleteCharValue = useDeleteCharacteristicValue()

  const [relDialogOpen, setRelDialogOpen] = useState(false)
  const [relForm, setRelForm] = useState({
    targetSpecificationId: "",
    relationshipType: "",
    role: "",
    validFrom: "",
    validTo: "",
  })
  const createRel = useCreateSpecificationRelationship()
  const deleteRel = useDeleteSpecificationRelationship()

  const handleAddCharacteristic = () => {
    createChar.mutate(
      {
        specId: id,
        name: charForm.name,
        description: charForm.description || null,
        valueType: charForm.valueType,
        configurable: charForm.configurable,
        sortOrder: charForm.sortOrder,
        isRequired: charForm.isRequired,
        minValue: charForm.minValue ? Number(charForm.minValue) : null,
        maxValue: charForm.maxValue ? Number(charForm.maxValue) : null,
        regex: null,
        maxCardinality: null,
      },
      {
        onSuccess: () => {
          toast({ title: "Characteristic created" })
          setCharDialogOpen(false)
          setCharForm({ name: "", description: "", valueType: "", configurable: false, isRequired: false, sortOrder: 0, minValue: "", maxValue: "" })
        },
        onError: () => {
          toast({ title: "Error", description: "Failed to create characteristic.", variant: "destructive" })
        },
      }
    )
  }

  const handleAddValue = () => {
    if (!expandedCharId) return
    createCharValue.mutate(
      {
        specId: id,
        characteristicId: expandedCharId,
        value: valueForm.value,
        unitOfMeasure: valueForm.unitOfMeasure || null,
        isDefault: valueForm.isDefault,
        valueFrom: null,
        valueTo: null,
        rangeInterval: null,
        validFrom: null,
        validTo: null,
      },
      {
        onSuccess: () => {
          toast({ title: "Value added" })
          setValueDialogOpen(false)
          setValueForm({ value: "", unitOfMeasure: "", isDefault: false })
        },
        onError: () => {
          toast({ title: "Error", description: "Failed to add value.", variant: "destructive" })
        },
      }
    )
  }

  const handleAddRelationship = () => {
    createRel.mutate(
      {
        specId: id,
        targetSpecificationId: relForm.targetSpecificationId,
        relationshipType: relForm.relationshipType,
        role: relForm.role || null,
        validFrom: relForm.validFrom || null,
        validTo: relForm.validTo || null,
      },
      {
        onSuccess: () => {
          toast({ title: "Relationship created" })
          setRelDialogOpen(false)
          setRelForm({ targetSpecificationId: "", relationshipType: "", role: "", validFrom: "", validTo: "" })
        },
        onError: () => {
          toast({ title: "Error", description: "Failed to create relationship.", variant: "destructive" })
        },
      }
    )
  }

  const expandedChar = expandedCharId
    ? spec?.characteristics.find((c) => c.id === expandedCharId)
    : null

  const characteristicColumns: Column<ProductSpecificationCharacteristicDto>[] = [
    {
      id: "expand",
      header: "",
      cell: (row) =>
        expandedCharId === row.id ? (
          <ChevronDown className="h-4 w-4" />
        ) : (
          <ChevronRight className="h-4 w-4" />
        ),
      width: "32px",
    },
    { id: "name", header: "Name", accessorKey: "name" },
    { id: "valueType", header: "ValueType", accessorKey: "valueType" },
    { id: "configurable", header: "Configurable", cell: (row) => (row.configurable ? "Yes" : "No") },
    { id: "isRequired", header: "Required", cell: (row) => (row.isRequired ? "Yes" : "No") },
    { id: "sortOrder", header: "SortOrder", accessorKey: "sortOrder" },
    {
      id: "values",
      header: "Values",
      cell: (row) =>
        row.values.length > 0 ? (
          <div className="space-y-1">
            {row.values.map((v) => (
              <div key={v.id} className="text-sm">
                <span className="font-medium">{v.value}</span>
                {v.unitOfMeasure && <span className="text-muted-foreground ml-1">{v.unitOfMeasure}</span>}
                {v.isDefault && <span className="text-xs text-muted-foreground ml-1">(default)</span>}
              </div>
            ))}
          </div>
        ) : (
          <span className="text-sm text-muted-foreground">-</span>
        ),
    },
    {
      id: "actions",
      header: "",
      cell: (row) => (
        <Button
          variant="ghost"
          size="icon"
          onClick={(e) => {
            e.stopPropagation()
            deleteChar.mutate(
              { specId: id, charId: row.id },
              {
                onSuccess: () => toast({ title: "Characteristic deleted" }),
                onError: () =>
                  toast({ title: "Error", description: "Failed to delete characteristic.", variant: "destructive" }),
              }
            )
          }}
        >
          <Trash2 className="h-4 w-4" />
        </Button>
      ),
      width: "48px",
    },
  ]

  const relationshipColumns: Column<ProductSpecificationRelationshipDto>[] = [
    { id: "relationshipType", header: "RelationshipType", accessorKey: "relationshipType" },
    { id: "targetSpecificationId", header: "TargetSpec", accessorKey: "targetSpecificationId" },
    { id: "role", header: "Role", cell: (row) => row.role ?? "-" },
    {
      id: "validFrom",
      header: "ValidFrom",
      cell: (row) => (row.validFrom ? new Date(row.validFrom).toLocaleDateString() : "-"),
    },
    {
      id: "validTo",
      header: "ValidTo",
      cell: (row) => (row.validTo ? new Date(row.validTo).toLocaleDateString() : "-"),
    },
    {
      id: "actions",
      header: "",
      cell: (row) => (
        <Button
          variant="ghost"
          size="icon"
          onClick={(e) => {
            e.stopPropagation()
            deleteRel.mutate(
              { specId: id, relId: row.id },
              {
                onSuccess: () => toast({ title: "Relationship deleted" }),
                onError: () =>
                  toast({ title: "Error", description: "Failed to delete relationship.", variant: "destructive" }),
              }
            )
          }}
        >
          <Trash2 className="h-4 w-4" />
        </Button>
      ),
      width: "48px",
    },
  ]

  const tabs = [
    {
      id: "overview",
      label: "Overview",
      content: (
        <EntityMetadata
          title="Product Specification Details"
          loading={isLoading}
          fields={[
            { label: "Name", value: spec?.name ?? "-" },
            { label: "Description", value: spec?.description ?? "-" },
            { label: "Brand", value: spec?.brand ?? "-" },
            { label: "Version", value: spec?.version ?? "-" },
            { label: "Product Number", value: spec?.productNumber ?? "-" },
            { label: "Lifecycle Status", value: spec ? <StatusBadge status={spec.lifecycleStatus} /> : "-" },
            { label: "Created", value: spec?.createdAt ? new Date(spec.createdAt).toLocaleDateString() : "-" },
          ]}
        />
      ),
    },
    {
      id: "characteristics",
      label: "Characteristics",
      content: (
        <Card>
          <CardHeader>
            <div className="flex items-center justify-between">
              <CardTitle className="text-base">Characteristics</CardTitle>
              <Dialog open={charDialogOpen} onOpenChange={setCharDialogOpen}>
                <DialogTrigger asChild>
                  <Button size="sm">
                    <Plus className="mr-2 h-4 w-4" />
                    Add Characteristic
                  </Button>
                </DialogTrigger>
                <DialogContent>
                  <DialogHeader>
                    <DialogTitle>Add Characteristic</DialogTitle>
                  </DialogHeader>
                  <div className="grid gap-4 py-4">
                    <div className="grid gap-2">
                      <Label htmlFor="char-name">Name</Label>
                      <Input
                        id="char-name"
                        value={charForm.name}
                        onChange={(e) => setCharForm({ ...charForm, name: e.target.value })}
                      />
                    </div>
                    <div className="grid gap-2">
                      <Label htmlFor="char-description">Description</Label>
                      <Textarea
                        id="char-description"
                        value={charForm.description}
                        onChange={(e) => setCharForm({ ...charForm, description: e.target.value })}
                      />
                    </div>
                    <div className="grid gap-2">
                      <Label htmlFor="char-valueType">Value Type</Label>
                      <Input
                        id="char-valueType"
                        value={charForm.valueType}
                        onChange={(e) => setCharForm({ ...charForm, valueType: e.target.value })}
                      />
                    </div>
                    <div className="grid grid-cols-2 gap-4">
                      <div className="flex items-center gap-2">
                        <Checkbox
                          id="char-configurable"
                          checked={charForm.configurable}
                          onCheckedChange={(checked) => setCharForm({ ...charForm, configurable: !!checked })}
                        />
                        <Label htmlFor="char-configurable">Configurable</Label>
                      </div>
                      <div className="flex items-center gap-2">
                        <Checkbox
                          id="char-isRequired"
                          checked={charForm.isRequired}
                          onCheckedChange={(checked) => setCharForm({ ...charForm, isRequired: !!checked })}
                        />
                        <Label htmlFor="char-isRequired">Required</Label>
                      </div>
                    </div>
                    <div className="grid gap-2">
                      <Label htmlFor="char-sortOrder">Sort Order</Label>
                      <Input
                        id="char-sortOrder"
                        type="number"
                        value={charForm.sortOrder}
                        onChange={(e) => setCharForm({ ...charForm, sortOrder: Number(e.target.value) })}
                      />
                    </div>
                    <div className="grid grid-cols-2 gap-4">
                      <div className="grid gap-2">
                        <Label htmlFor="char-minValue">Min Value (optional)</Label>
                        <Input
                          id="char-minValue"
                          type="number"
                          value={charForm.minValue}
                          onChange={(e) => setCharForm({ ...charForm, minValue: e.target.value })}
                        />
                      </div>
                      <div className="grid gap-2">
                        <Label htmlFor="char-maxValue">Max Value (optional)</Label>
                        <Input
                          id="char-maxValue"
                          type="number"
                          value={charForm.maxValue}
                          onChange={(e) => setCharForm({ ...charForm, maxValue: e.target.value })}
                        />
                      </div>
                    </div>
                    <Button onClick={handleAddCharacteristic} disabled={createChar.isPending}>
                      {createChar.isPending ? "Saving..." : "Save"}
                    </Button>
                  </div>
                </DialogContent>
              </Dialog>
            </div>
          </CardHeader>
          <CardContent className="space-y-4">
            <DataTable
              columns={characteristicColumns}
              data={spec?.characteristics ?? []}
              loading={isLoading}
              rowKey={(row) => row.id}
              onRowClick={(row) => setExpandedCharId(expandedCharId === row.id ? null : row.id)}
            />
            {expandedChar && (
              <div className="rounded-md border p-4">
                <div className="mb-3 flex items-center justify-between">
                  <h4 className="text-sm font-medium">Values for {expandedChar.name}</h4>
                  <Dialog open={valueDialogOpen} onOpenChange={setValueDialogOpen}>
                    <DialogTrigger asChild>
                      <Button size="sm" variant="outline">
                        <Plus className="mr-2 h-4 w-4" />
                        Add Value
                      </Button>
                    </DialogTrigger>
                    <DialogContent>
                      <DialogHeader>
                        <DialogTitle>Add Characteristic Value</DialogTitle>
                      </DialogHeader>
                      <div className="grid gap-4 py-4">
                        <div className="grid gap-2">
                          <Label htmlFor="value-value">Value</Label>
                          <Input
                            id="value-value"
                            value={valueForm.value}
                            onChange={(e) => setValueForm({ ...valueForm, value: e.target.value })}
                          />
                        </div>
                        <div className="grid gap-2">
                          <Label htmlFor="value-unitOfMeasure">Unit of Measure (optional)</Label>
                          <Input
                            id="value-unitOfMeasure"
                            value={valueForm.unitOfMeasure}
                            onChange={(e) => setValueForm({ ...valueForm, unitOfMeasure: e.target.value })}
                          />
                        </div>
                        <div className="flex items-center gap-2">
                          <Checkbox
                            id="value-isDefault"
                            checked={valueForm.isDefault}
                            onCheckedChange={(checked) => setValueForm({ ...valueForm, isDefault: !!checked })}
                          />
                          <Label htmlFor="value-isDefault">Default</Label>
                        </div>
                        <Button onClick={handleAddValue} disabled={createCharValue.isPending}>
                          {createCharValue.isPending ? "Saving..." : "Save"}
                        </Button>
                      </div>
                    </DialogContent>
                  </Dialog>
                </div>
                {expandedChar.values.length === 0 ? (
                  <p className="text-sm text-muted-foreground">No values yet.</p>
                ) : (
                  <div className="space-y-2">
                    {expandedChar.values.map((v) => (
                      <div key={v.id} className="flex items-center justify-between rounded-md border px-3 py-2">
                        <div className="text-sm">
                          <span className="font-medium">{v.value}</span>
                          {v.unitOfMeasure && <span className="text-muted-foreground ml-1">{v.unitOfMeasure}</span>}
                          {v.isDefault && <span className="text-xs text-muted-foreground ml-1">(default)</span>}
                        </div>
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() =>
                            deleteCharValue.mutate(
                              { specId: id, charId: expandedChar.id, valueId: v.id },
                              {
                                onSuccess: () => toast({ title: "Value deleted" }),
                                onError: () =>
                                  toast({
                                    title: "Error",
                                    description: "Failed to delete value.",
                                    variant: "destructive",
                                  }),
                              }
                            )
                          }
                        >
                          <Trash2 className="h-4 w-4" />
                        </Button>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            )}
          </CardContent>
        </Card>
      ),
    },
    {
      id: "relationships",
      label: "Relationships",
      content: (
        <Card>
          <CardHeader>
            <div className="flex items-center justify-between">
              <CardTitle className="text-base">Relationships</CardTitle>
              <Dialog open={relDialogOpen} onOpenChange={setRelDialogOpen}>
                <DialogTrigger asChild>
                  <Button size="sm">
                    <Plus className="mr-2 h-4 w-4" />
                    Add Relationship
                  </Button>
                </DialogTrigger>
                <DialogContent>
                  <DialogHeader>
                    <DialogTitle>Add Relationship</DialogTitle>
                  </DialogHeader>
                  <div className="grid gap-4 py-4">
                    <div className="grid gap-2">
                      <Label htmlFor="rel-targetSpecificationId">Target Specification ID</Label>
                      <Input
                        id="rel-targetSpecificationId"
                        value={relForm.targetSpecificationId}
                        onChange={(e) => setRelForm({ ...relForm, targetSpecificationId: e.target.value })}
                      />
                    </div>
                    <div className="grid gap-2">
                      <Label htmlFor="rel-relationshipType">Relationship Type</Label>
                      <Input
                        id="rel-relationshipType"
                        placeholder="e.g. dependency, alternative, substitution"
                        value={relForm.relationshipType}
                        onChange={(e) => setRelForm({ ...relForm, relationshipType: e.target.value })}
                      />
                    </div>
                    <div className="grid gap-2">
                      <Label htmlFor="rel-role">Role (optional)</Label>
                      <Input
                        id="rel-role"
                        value={relForm.role}
                        onChange={(e) => setRelForm({ ...relForm, role: e.target.value })}
                      />
                    </div>
                    <div className="grid grid-cols-2 gap-4">
                      <div className="grid gap-2">
                        <Label htmlFor="rel-validFrom">Valid From (optional)</Label>
                        <Input
                          id="rel-validFrom"
                          type="date"
                          value={relForm.validFrom}
                          onChange={(e) => setRelForm({ ...relForm, validFrom: e.target.value })}
                        />
                      </div>
                      <div className="grid gap-2">
                        <Label htmlFor="rel-validTo">Valid To (optional)</Label>
                        <Input
                          id="rel-validTo"
                          type="date"
                          value={relForm.validTo}
                          onChange={(e) => setRelForm({ ...relForm, validTo: e.target.value })}
                        />
                      </div>
                    </div>
                    <Button onClick={handleAddRelationship} disabled={createRel.isPending}>
                      {createRel.isPending ? "Saving..." : "Save"}
                    </Button>
                  </div>
                </DialogContent>
              </Dialog>
            </div>
          </CardHeader>
          <CardContent>
            <DataTable
              columns={relationshipColumns}
              data={spec?.relationships ?? []}
              loading={isLoading}
              rowKey={(row) => row.id}
            />
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
            {(auditEntries ?? []).length === 0 ? (
              <p className="text-sm text-muted-foreground">No audit entries found.</p>
            ) : (
              auditEntries?.map((entry) => (
                <div key={entry.id} className="border-b py-3">
                  <div className="flex justify-between">
                    <span className="font-medium">{entry.action}</span>
                    <span className="text-sm text-muted-foreground">{new Date(entry.performedAt).toLocaleString()}</span>
                  </div>
                  <p className="text-sm text-muted-foreground">By: {entry.performedByName ?? "-"}</p>
                </div>
              ))
            )}
          </CardContent>
        </Card>
      ),
    },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <EntityHeader
        title={spec?.name ?? "Product Specification"}
        subtitle={spec?.description ?? undefined}
        status={spec?.lifecycleStatus}
        backHref="/product-specifications"
        editHref={`/product-specifications/${id}/edit`}
        onDelete={() => {
          if (confirm("Are you sure you want to delete this product specification?")) {
            deleteSpec.mutate(id, {
              onSuccess: () => {
                toast({ title: "Product Specification deleted", description: "Product Specification has been deleted." })
                queryClient.invalidateQueries({ queryKey: queryKeys.productSpecifications.lists() })
                router.push("/product-specifications")
              },
              onError: () => {
                toast({ title: "Error", description: "Failed to delete product specification.", variant: "destructive" })
              },
            })
          }
        }}
        loading={isLoading}
      />

      <EntityTabs tabs={tabs} defaultTab="overview" />
    </div>
  )
}
