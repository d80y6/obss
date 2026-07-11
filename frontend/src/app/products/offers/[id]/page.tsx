"use client"

import { useParams } from "next/navigation"
import { useRouter } from "next/navigation"
import { useState } from "react"
import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { EntityTabs } from "@/components/shared/EntityTabs"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { DataTable } from "@/components/shared/DataTable"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Textarea } from "@/components/ui/textarea"
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Plus, Trash2 } from "lucide-react"
import { useOffer } from "@/api/hooks/useOffer"
import { useDeleteOffer } from "@/api/hooks/useDeleteOffer"
import { useCreateTerm } from "@/api/hooks/useCreateTerm"
import { useDeleteTerm } from "@/api/hooks/useDeleteTerm"
import { useCreateBundledOffering } from "@/api/hooks/useCreateBundledOffering"
import { useDeleteBundledOffering } from "@/api/hooks/useDeleteBundledOffering"
import { useCreatePriceRange } from "@/api/hooks/useCreatePriceRange"
import { useDeletePriceRange } from "@/api/hooks/useDeletePriceRange"
import { toast } from "@/components/ui/toast"
import { useAuditLog } from "@/api/hooks/useAuditLog"
import { queryKeys } from "@/lib/query-keys"
import { useQueryClient } from "@tanstack/react-query"
import type { Column } from "@/components/shared/DataTable"
import type { ProductOfferingTermDto, BundledProductOfferingDto, OfferPricingDto } from "@/api/generated/dto"

export default function OfferDetailPage() {
  const params = useParams()
  const router = useRouter()
  const queryClient = useQueryClient()
  const id = params.id as string

  const { data: offer, isLoading } = useOffer(id)
  const deleteOffer = useDeleteOffer()

  const { data: auditEntries } = useAuditLog("Offer", id)

  const createTerm = useCreateTerm()
  const deleteTerm = useDeleteTerm()
  const createBundled = useCreateBundledOffering()
  const deleteBundled = useDeleteBundledOffering()
  const createPriceRange = useCreatePriceRange()
  const deletePriceRange = useDeletePriceRange()

  const [termDialogOpen, setTermDialogOpen] = useState(false)
  const [bundledDialogOpen, setBundledDialogOpen] = useState(false)
  const [priceRangeDialogOpen, setPriceRangeDialogOpen] = useState(false)
  const [selectedPricingId, setSelectedPricingId] = useState<string | null>(null)

  const [termName, setTermName] = useState("")
  const [termDescription, setTermDescription] = useState("")
  const [termDuration, setTermDuration] = useState("")
  const [termDurationUnit, setTermDurationUnit] = useState("MONTH")
  const [termType, setTermType] = useState("")
  const [termValidFrom, setTermValidFrom] = useState("")
  const [termValidTo, setTermValidTo] = useState("")

  const [bundledOfferId, setBundledOfferId] = useState("")
  const [bundledName, setBundledName] = useState("")
  const [bundledQuantity, setBundledQuantity] = useState("")
  const [bundledReferralType, setBundledReferralType] = useState("")

  const [prMinQuantity, setPrMinQuantity] = useState("")
  const [prMaxQuantity, setPrMaxQuantity] = useState("")
  const [prPrice, setPrPrice] = useState("")

  const resetTermForm = () => {
    setTermName("")
    setTermDescription("")
    setTermDuration("")
    setTermDurationUnit("MONTH")
    setTermType("")
    setTermValidFrom("")
    setTermValidTo("")
  }

  const resetBundledForm = () => {
    setBundledOfferId("")
    setBundledName("")
    setBundledQuantity("")
    setBundledReferralType("")
  }

  const resetPriceRangeForm = () => {
    setPrMinQuantity("")
    setPrMaxQuantity("")
    setPrPrice("")
  }

  const handleAddTerm = () => {
    createTerm.mutate({
      offerId: id,
      name: termName,
      description: termDescription || null,
      duration: parseInt(termDuration),
      durationUnit: termDurationUnit,
      termType: termType,
      validFrom: termValidFrom || null,
      validTo: termValidTo || null,
    }, {
      onSuccess: () => {
        toast({ title: "Term added", description: "Term has been added successfully." })
        setTermDialogOpen(false)
        resetTermForm()
      },
      onError: () => {
        toast({ title: "Error", description: "Failed to add term.", variant: "destructive" })
      },
    })
  }

  const handleAddBundled = () => {
    createBundled.mutate({
      offerId: id,
      bundledOfferId: bundledOfferId,
      name: bundledName || null,
      quantity: parseInt(bundledQuantity),
      referralType: bundledReferralType || null,
    }, {
      onSuccess: () => {
        toast({ title: "Bundled offering added", description: "Bundled offering has been added successfully." })
        setBundledDialogOpen(false)
        resetBundledForm()
      },
      onError: () => {
        toast({ title: "Error", description: "Failed to add bundled offering.", variant: "destructive" })
      },
    })
  }

  const handleAddPriceRange = () => {
    if (!selectedPricingId) return
    createPriceRange.mutate({
      offerId: id,
      pricingId: selectedPricingId,
      minQuantity: parseInt(prMinQuantity),
      maxQuantity: prMaxQuantity ? parseInt(prMaxQuantity) : null,
      price: parseFloat(prPrice),
    }, {
      onSuccess: () => {
        toast({ title: "Price range added", description: "Price range has been added successfully." })
        setPriceRangeDialogOpen(false)
        setSelectedPricingId(null)
        resetPriceRangeForm()
      },
      onError: () => {
        toast({ title: "Error", description: "Failed to add price range.", variant: "destructive" })
      },
    })
  }

  const termColumns: Column<ProductOfferingTermDto>[] = [
    { id: "name", header: "Name", accessorKey: "name" },
    { id: "duration", header: "Duration", cell: (row) => `${row.duration} ${row.durationUnit}` },
    { id: "termType", header: "Type", accessorKey: "termType" },
    { id: "validFrom", header: "Valid From", cell: (row) => row.validFrom ? new Date(row.validFrom).toLocaleDateString() : "-" },
    { id: "validTo", header: "Valid To", cell: (row) => row.validTo ? new Date(row.validTo).toLocaleDateString() : "-" },
    {
      id: "actions",
      header: "",
      cell: (row) => (
        <Button variant="ghost" size="icon" onClick={() => deleteTerm.mutate({ offerId: id, termId: row.id }, {
          onSuccess: () => toast({ title: "Term deleted" }),
          onError: () => toast({ title: "Error", description: "Failed to delete term.", variant: "destructive" }),
        })}>
          <Trash2 className="h-4 w-4" />
        </Button>
      ),
    },
  ]

  const bundledColumns: Column<BundledProductOfferingDto>[] = [
    { id: "name", header: "Name", accessorKey: "name" },
    { id: "quantity", header: "Quantity", accessorKey: "quantity" },
    { id: "referralType", header: "Referral Type", cell: (row) => row.referralType ?? "-" },
    {
      id: "actions",
      header: "",
      cell: (row) => (
        <Button variant="ghost" size="icon" onClick={() => deleteBundled.mutate({ offerId: id, bundledOfferingId: row.id }, {
          onSuccess: () => toast({ title: "Bundled offering deleted" }),
          onError: () => toast({ title: "Error", description: "Failed to delete bundled offering.", variant: "destructive" }),
        })}>
          <Trash2 className="h-4 w-4" />
        </Button>
      ),
    },
  ]

  const pricingColumns: Column<OfferPricingDto>[] = [
    { id: "name", header: "Name", cell: (row) => row.name ?? "-" },
    { id: "pricingType", header: "Type", accessorKey: "pricingType" },
    { id: "currency", header: "Currency", accessorKey: "currency" },
    { id: "recurringPrice", header: "Recurring", cell: (row) => row.recurringPrice > 0 ? `$${row.recurringPrice.toFixed(2)}` : "-" },
    { id: "oneTimePrice", header: "One-Time", cell: (row) => row.oneTimePrice > 0 ? `$${row.oneTimePrice.toFixed(2)}` : "-" },
    { id: "usagePrice", header: "Usage", cell: (row) => row.usagePrice > 0 ? `$${row.usagePrice.toFixed(2)}` : "-" },
    { id: "status", header: "Status", cell: (row) => <StatusBadge status={row.isActive ? "Active" : "Inactive"} /> },
    {
      id: "priceRanges",
      header: "Price Ranges",
      cell: (row) => (
        <div className="space-y-2">
          {row.priceRanges && row.priceRanges.length > 0 && (
            <div className="space-y-1">
              {row.priceRanges.map((range) => (
                <div key={range.id} className="flex items-center gap-2 text-sm">
                  <span>{range.minQuantity}-{range.maxQuantity ?? '∞'}: ${range.price.toFixed(2)}</span>
                  <Button variant="ghost" size="icon" className="h-6 w-6" onClick={() => deletePriceRange.mutate({ offerId: id, pricingId: row.id, rangeId: range.id }, {
                    onSuccess: () => toast({ title: "Price range deleted" }),
                    onError: () => toast({ title: "Error", description: "Failed to delete price range.", variant: "destructive" }),
                  })}>
                    <Trash2 className="h-3 w-3" />
                  </Button>
                </div>
              ))}
            </div>
          )}
          <Button size="sm" variant="outline" onClick={() => { setSelectedPricingId(row.id); setPriceRangeDialogOpen(true) }}>
            <Plus className="h-4 w-4" /> Add Range
          </Button>
        </div>
      ),
    },
  ]

  const tabs = [
    {
      id: "overview",
      label: "Overview",
      content: (
        <EntityMetadata
          title="Offer Details"
          loading={isLoading}
          fields={[
            { label: "Name", value: offer?.name ?? "-" },
            { label: "Description", value: offer?.description ?? "-" },
            { label: "Offer Type", value: offer?.offerType ?? "-" },
            { label: "Price", value: offer?.pricings?.[0] ? `${offer.pricings[0].currency} ${(offer.pricings[0].recurringPrice || offer.pricings[0].oneTimePrice || offer.pricings[0].usagePrice || 0).toFixed(2)}` : "-" },
            { label: "Billing Period", value: offer?.billingPeriod ?? "-" },
            { label: "Status", value: offer ? <StatusBadge status={offer.isActive ? "Active" : "Inactive"} /> : "-" },
            { label: "Created", value: offer?.createdAt ? new Date(offer.createdAt).toLocaleDateString() : "-" },
          ]}
        />
      ),
    },
    {
      id: "terms",
      label: "Terms",
      content: (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Terms &amp; Conditions</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <Dialog open={termDialogOpen} onOpenChange={(open) => { setTermDialogOpen(open); if (!open) resetTermForm() }}>
              <DialogTrigger asChild>
                <Button><Plus className="h-4 w-4" /> Add Term</Button>
              </DialogTrigger>
              <DialogContent>
                <DialogHeader>
                  <DialogTitle>Add Term</DialogTitle>
                </DialogHeader>
                <div className="grid gap-4">
                  <div className="grid gap-2">
                    <Label htmlFor="termName">Name</Label>
                    <Input id="termName" value={termName} onChange={(e) => setTermName(e.target.value)} />
                  </div>
                  <div className="grid gap-2">
                    <Label htmlFor="termDescription">Description</Label>
                    <Textarea id="termDescription" value={termDescription} onChange={(e) => setTermDescription(e.target.value)} />
                  </div>
                  <div className="grid gap-2">
                    <Label htmlFor="termDuration">Duration</Label>
                    <Input id="termDuration" type="number" value={termDuration} onChange={(e) => setTermDuration(e.target.value)} />
                  </div>
                  <div className="grid gap-2">
                    <Label htmlFor="termDurationUnit">Duration Unit</Label>
                    <Select value={termDurationUnit} onValueChange={setTermDurationUnit}>
                      <SelectTrigger id="termDurationUnit">
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="DAY">Day</SelectItem>
                        <SelectItem value="MONTH">Month</SelectItem>
                        <SelectItem value="YEAR">Year</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>
                  <div className="grid gap-2">
                    <Label htmlFor="termType">Term Type</Label>
                    <Input id="termType" value={termType} onChange={(e) => setTermType(e.target.value)} />
                  </div>
                  <div className="grid gap-2">
                    <Label htmlFor="termValidFrom">Valid From</Label>
                    <Input id="termValidFrom" type="date" value={termValidFrom} onChange={(e) => setTermValidFrom(e.target.value)} />
                  </div>
                  <div className="grid gap-2">
                    <Label htmlFor="termValidTo">Valid To</Label>
                    <Input id="termValidTo" type="date" value={termValidTo} onChange={(e) => setTermValidTo(e.target.value)} />
                  </div>
                  <Button onClick={handleAddTerm}>Save</Button>
                </div>
              </DialogContent>
            </Dialog>
            <DataTable
              columns={termColumns}
              data={offer?.terms ?? []}
              loading={isLoading}
              rowKey={(row) => row.id}
              emptyTitle="No terms defined"
              emptyDescription="Add terms to specify contract durations and conditions."
            />
          </CardContent>
        </Card>
      ),
    },
    {
      id: "bundled-offerings",
      label: "Bundled",
      content: (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Bundled Offerings</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <Dialog open={bundledDialogOpen} onOpenChange={(open) => { setBundledDialogOpen(open); if (!open) resetBundledForm() }}>
              <DialogTrigger asChild>
                <Button><Plus className="h-4 w-4" /> Add Bundled</Button>
              </DialogTrigger>
              <DialogContent>
                <DialogHeader>
                  <DialogTitle>Add Bundled Offering</DialogTitle>
                </DialogHeader>
                <div className="grid gap-4">
                  <div className="grid gap-2">
                    <Label htmlFor="bundledOfferId">Bundled Offer ID</Label>
                    <Input id="bundledOfferId" value={bundledOfferId} onChange={(e) => setBundledOfferId(e.target.value)} />
                  </div>
                  <div className="grid gap-2">
                    <Label htmlFor="bundledName">Name</Label>
                    <Input id="bundledName" value={bundledName} onChange={(e) => setBundledName(e.target.value)} />
                  </div>
                  <div className="grid gap-2">
                    <Label htmlFor="bundledQuantity">Quantity</Label>
                    <Input id="bundledQuantity" type="number" value={bundledQuantity} onChange={(e) => setBundledQuantity(e.target.value)} />
                  </div>
                  <div className="grid gap-2">
                    <Label htmlFor="bundledReferralType">Referral Type</Label>
                    <Input id="bundledReferralType" value={bundledReferralType} onChange={(e) => setBundledReferralType(e.target.value)} />
                  </div>
                  <Button onClick={handleAddBundled}>Save</Button>
                </div>
              </DialogContent>
            </Dialog>
            <DataTable
              columns={bundledColumns}
              data={offer?.bundledOfferings ?? []}
              loading={isLoading}
              rowKey={(row) => row.id}
              emptyTitle="No bundled offerings"
              emptyDescription="Add bundled offerings to create product bundles."
            />
          </CardContent>
        </Card>
      ),
    },
    {
      id: "pricing",
      label: "Pricing",
      content: (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Price Configurations</CardTitle>
          </CardHeader>
          <CardContent>
            <DataTable
              columns={pricingColumns}
              data={offer?.pricings ?? []}
              loading={isLoading}
              rowKey={(row) => row.id}
              emptyTitle="No pricing defined"
              emptyDescription="Add pricing to define how this offer is charged."
            />
            <Dialog open={priceRangeDialogOpen} onOpenChange={(open) => { setPriceRangeDialogOpen(open); if (!open) { setSelectedPricingId(null); resetPriceRangeForm() } }}>
              <DialogContent>
                <DialogHeader>
                  <DialogTitle>Add Price Range</DialogTitle>
                </DialogHeader>
                <div className="grid gap-4">
                  <div className="grid gap-2">
                    <Label htmlFor="prMinQuantity">Min Quantity</Label>
                    <Input id="prMinQuantity" type="number" value={prMinQuantity} onChange={(e) => setPrMinQuantity(e.target.value)} />
                  </div>
                  <div className="grid gap-2">
                    <Label htmlFor="prMaxQuantity">Max Quantity</Label>
                    <Input id="prMaxQuantity" type="number" value={prMaxQuantity} onChange={(e) => setPrMaxQuantity(e.target.value)} />
                  </div>
                  <div className="grid gap-2">
                    <Label htmlFor="prPrice">Price</Label>
                    <Input id="prPrice" type="number" step="0.01" value={prPrice} onChange={(e) => setPrPrice(e.target.value)} />
                  </div>
                  <Button onClick={handleAddPriceRange}>Save</Button>
                </div>
              </DialogContent>
            </Dialog>
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
                  <p className="text-sm text-muted-foreground">By: {entry.performedByName}</p>
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
        title={offer?.name ?? "Offer"}
        subtitle={offer?.description ?? undefined}
        status={offer?.isActive ? "Active" : "Inactive"}
        backHref="/products/offers"
        editHref={`/products/offers/${id}/edit`}
        onDelete={() => {
          if (confirm("Are you sure you want to delete this offer?")) {
            deleteOffer.mutate(id, {
              onSuccess: () => {
                toast({ title: "Offer deleted", description: "Offer has been deleted." })
                queryClient.invalidateQueries({ queryKey: queryKeys.offers.lists() })
                router.push("/products/offers")
              },
              onError: () => {
                toast({ title: "Error", description: "Failed to delete offer.", variant: "destructive" })
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
