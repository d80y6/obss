"use client"

import { useState } from "react"
import { useRouter } from "next/navigation"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { SearchBar } from "@/components/shared/SearchBar"
import { toast } from "@/components/ui/toast"
import type { CustomerDto, ProductDto } from "@/api/generated"
import { useProducts } from "@/api/hooks/useProducts"
import { useCustomers } from "@/api/hooks/useCustomers"
import { useCreateOrder } from "@/api/hooks/useCreateOrder"

const ORDER_TYPES = [
  { value: "New", label: "New" },
  { value: "Renewal", label: "Renewal" },
  { value: "Change", label: "Change" },
  { value: "Termination", label: "Termination" },
  { value: "Transfer", label: "Transfer" },
]

const CURRENCIES = [
  { value: "USD", label: "USD ($)" },
  { value: "EUR", label: "EUR (€)" },
  { value: "GBP", label: "GBP (£)" },
  { value: "TRY", label: "TRY (₺)" },
  { value: "AED", label: "AED (د.إ)" },
]

const BILLING_PERIODS = [
  { value: "Monthly", label: "Monthly" },
  { value: "Quarterly", label: "Quarterly" },
  { value: "SemiAnnual", label: "Semi-Annual" },
  { value: "Annual", label: "Annual" },
]

interface OrderItem {
  productId: string
  offerId: string
  productName: string
  offerName: string
  quantity: number
  unitPrice: number
  recurringPrice: number
  discountAmount: number
  taxAmount: number
  billingPeriod: string
}

export default function NewOrderPage() {
  const router = useRouter()
  const [step, setStep] = useState(1)
  const [customerSearch, setCustomerSearch] = useState("")
  const [selectedCustomer, setSelectedCustomer] = useState<CustomerDto | null>(null)
  const [items, setItems] = useState<OrderItem[]>([])
  const [notes, setNotes] = useState("")
  const [orderType, setOrderType] = useState("New")
  const [currency, setCurrency] = useState("USD")
  const [billingAddressStreet, setBillingAddressStreet] = useState("")
  const [billingAddressCity, setBillingAddressCity] = useState("")
  const [billingAddressState, setBillingAddressState] = useState("")
  const [billingAddressPostalCode, setBillingAddressPostalCode] = useState("")
  const [billingAddressCountry, setBillingAddressCountry] = useState("")
  const [shippingAddressStreet, setShippingAddressStreet] = useState("")
  const [shippingAddressCity, setShippingAddressCity] = useState("")
  const [shippingAddressState, setShippingAddressState] = useState("")
  const [shippingAddressPostalCode, setShippingAddressPostalCode] = useState("")
  const [shippingAddressCountry, setShippingAddressCountry] = useState("")
  const [showAddressFields, setShowAddressFields] = useState(false)

  const { data: customersData } = useCustomers(customerSearch ? { search: customerSearch } : {})

  const { data: products } = useProducts()

  const customerColumns: Column<CustomerDto>[] = [
    { id: "displayName", header: "Name", accessorKey: "displayName" },
    { id: "email", header: "Email", accessorKey: "email" },
    { id: "customerType", header: "Type", accessorKey: "customerType" },
  ]

  const createMutation = useCreateOrder()

  const handleCreateOrder = () => {
    const customer = selectedCustomer!
    createMutation.mutate({
      customerId: customer.id,
      customerName: customer.displayName,
      orderType,
      currency,
      notes,
      billingAddressStreet: billingAddressStreet || null,
      billingAddressCity: billingAddressCity || null,
      billingAddressState: billingAddressState || null,
      billingAddressPostalCode: billingAddressPostalCode || null,
      billingAddressCountry: billingAddressCountry || null,
      shippingAddressStreet: shippingAddressStreet || null,
      shippingAddressCity: shippingAddressCity || null,
      shippingAddressState: shippingAddressState || null,
      shippingAddressPostalCode: shippingAddressPostalCode || null,
      shippingAddressCountry: shippingAddressCountry || null,
      items: items.map((item) => ({
        productId: item.productId,
        offerId: item.offerId,
        productName: item.productName,
        offerName: item.offerName,
        quantity: item.quantity,
        unitPrice: item.unitPrice,
        recurringPrice: item.recurringPrice,
        discountAmount: item.discountAmount,
        taxAmount: item.taxAmount,
        billingPeriod: item.billingPeriod,
      })),
    }, {
      onSuccess: (data) => {
        toast({ title: "Order created", description: `Order ${data.orderNumber} has been created.` })
        router.push(`/orders/${data.id}`)
      },
      onError: () => {
        toast({ title: "Error", description: "Failed to create order.", variant: "destructive" })
      },
    })
  }

  const productList = products?.items ?? []

  const addItem = (product: ProductDto) => {
    setItems((prev) => {
      const existing = prev.find((i) => i.productId === product.id)
      if (existing) {
        return prev.map((i) =>
          i.productId === product.id ? { ...i, quantity: i.quantity + 1 } : i
        )
      }
      return [...prev, { productId: product.id, offerId: product.id, productName: product.name, offerName: product.name, quantity: 1, unitPrice: 0, recurringPrice: 0, discountAmount: 0, taxAmount: 0, billingPeriod: "Monthly" }]
    })
  }

  const updateItemField = (productId: string, field: keyof OrderItem, value: string | number) => {
    setItems((prev) =>
      prev.map((i) => (i.productId === productId ? { ...i, [field]: value } : i))
    )
  }

  const removeItem = (productId: string) => {
    setItems((prev) => prev.filter((i) => i.productId !== productId))
  }

  const totalAmount = items.reduce((sum, i) => sum + i.quantity * i.unitPrice, 0)

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="New Order" backHref="/orders" />

      <div className="flex items-center gap-2 mb-4">
        {[1, 2, 3].map((s) => (
          <div key={s} className="flex items-center">
            <div className={`w-8 h-8 rounded-full flex items-center justify-center text-sm font-medium ${step >= s ? "bg-primary text-primary-foreground" : "bg-muted text-muted-foreground"}`}>
              {s}
            </div>
            <span className={`ml-2 text-sm ${step >= s ? "font-medium" : "text-muted-foreground"}`}>
              {s === 1 ? "Select Customer" : s === 2 ? "Add Items" : "Review"}
            </span>
            {s < 3 && <div className="w-8 h-0.5 mx-2 bg-muted" />}
          </div>
        ))}
      </div>

      {step === 1 && (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Select Customer</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
              <SearchBar value={customerSearch} onChange={setCustomerSearch} placeholder="Search customers..." />
            <DataTable
              columns={customerColumns}
              data={customersData?.items ?? []}
              rowKey={(row) => row.id}
              onRowClick={(row) => {
                setSelectedCustomer(row)
              }}
              emptyTitle="No customers found"
            />
            {selectedCustomer && (
              <div className="p-3 border rounded-md bg-muted/50">
                <p className="text-sm font-medium">Selected: {selectedCustomer.displayName} ({selectedCustomer.email})</p>
              </div>
            )}
            <div className="flex justify-end">
              <Button onClick={() => setStep(2)} disabled={!selectedCustomer}>
                Next: Add Items
              </Button>
            </div>
          </CardContent>
        </Card>
      )}

      {step === 2 && (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Add Items</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid gap-2">
              <p className="text-sm font-medium">Available Products</p>
              <div className="flex flex-wrap gap-2">
                {(productList).map((product) => (
                  <Button key={product.id} variant="outline" size="sm" onClick={() => addItem(product)}>
                    + {product.name}
                  </Button>
                ))}
              </div>
            </div>

            {items.length > 0 && (
              <div className="space-y-2">
                <p className="text-sm font-medium">Order Items</p>
                {items.map((item) => (
                  <div key={item.productId} className="flex flex-wrap items-center gap-3 p-3 border rounded-md">
                    <span className="flex-1 text-sm font-medium min-w-[120px]">{item.productName}</span>
                    <div className="flex items-center gap-2">
                      <label className="text-xs text-muted-foreground">Qty:</label>
                      <Input
                        type="number"
                        className="w-16 h-8"
                        value={item.quantity}
                        onChange={(e) => updateItemField(item.productId, "quantity", parseInt(e.target.value) || 1)}
                        min={1}
                      />
                    </div>
                    <div className="flex items-center gap-2">
                      <label className="text-xs text-muted-foreground">Price:</label>
                      <Input
                        type="number"
                        className="w-24 h-8"
                        value={item.unitPrice}
                        onChange={(e) => updateItemField(item.productId, "unitPrice", parseFloat(e.target.value) || 0)}
                        min={0}
                        step={0.01}
                      />
                    </div>
                    <div className="flex items-center gap-2">
                      <label className="text-xs text-muted-foreground">Recurring:</label>
                      <Input
                        type="number"
                        className="w-20 h-8"
                        value={item.recurringPrice}
                        onChange={(e) => updateItemField(item.productId, "recurringPrice", parseFloat(e.target.value) || 0)}
                        min={0}
                        step={0.01}
                      />
                    </div>
                    <div className="flex items-center gap-2">
                      <label className="text-xs text-muted-foreground">Disc:</label>
                      <Input
                        type="number"
                        className="w-20 h-8"
                        value={item.discountAmount}
                        onChange={(e) => updateItemField(item.productId, "discountAmount", parseFloat(e.target.value) || 0)}
                        min={0}
                        step={0.01}
                      />
                    </div>
                    <div className="flex items-center gap-2">
                      <label className="text-xs text-muted-foreground">Tax:</label>
                      <Input
                        type="number"
                        className="w-20 h-8"
                        value={item.taxAmount}
                        onChange={(e) => updateItemField(item.productId, "taxAmount", parseFloat(e.target.value) || 0)}
                        min={0}
                        step={0.01}
                      />
                    </div>
                    <select
                      className="h-8 rounded-md border border-input bg-background px-2 text-xs"
                      value={item.billingPeriod}
                      onChange={(e) => updateItemField(item.productId, "billingPeriod", e.target.value)}
                    >
                      {BILLING_PERIODS.map((bp) => (
                        <option key={bp.value} value={bp.value}>{bp.label}</option>
                      ))}
                    </select>
                    <span className="text-sm font-medium w-20 text-right">
                      ${(item.quantity * item.unitPrice).toFixed(2)}
                    </span>
                    <Button variant="ghost" size="sm" onClick={() => removeItem(item.productId)}>Remove</Button>
                  </div>
                ))}
              </div>
            )}

            <div className="space-y-2">
              <label className="text-sm font-medium">Notes</label>
              <textarea
                className="flex min-h-[60px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm shadow-sm"
                value={notes}
                onChange={(e) => setNotes(e.target.value)}
                placeholder="Order notes..."
              />
            </div>

            <div className="flex justify-between">
              <Button variant="outline" onClick={() => setStep(1)}>Back</Button>
              <Button onClick={() => setStep(3)} disabled={items.length === 0}>
                Next: Review
              </Button>
            </div>
          </CardContent>
        </Card>
      )}

      {step === 3 && (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Review Order</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid gap-4 md:grid-cols-2">
              <div className="p-3 border rounded-md space-y-1">
                <p className="text-sm font-medium">Customer</p>
                <p className="text-sm text-muted-foreground">
                  {selectedCustomer?.displayName} ({selectedCustomer?.email})
                </p>
              </div>
              <div className="p-3 border rounded-md space-y-1">
                <p className="text-sm font-medium">Order Type</p>
                <select
                  className="w-full h-9 rounded-md border border-input bg-background px-3 text-sm"
                  value={orderType}
                  onChange={(e) => setOrderType(e.target.value)}
                >
                  {ORDER_TYPES.map((ot) => (
                    <option key={ot.value} value={ot.value}>{ot.label}</option>
                  ))}
                </select>
              </div>
              <div className="p-3 border rounded-md space-y-1">
                <p className="text-sm font-medium">Currency</p>
                <select
                  className="w-full h-9 rounded-md border border-input bg-background px-3 text-sm"
                  value={currency}
                  onChange={(e) => setCurrency(e.target.value)}
                >
                  {CURRENCIES.map((c) => (
                    <option key={c.value} value={c.value}>{c.label}</option>
                  ))}
                </select>
              </div>
            </div>

            <div className="p-3 border rounded-md space-y-2">
              <div className="flex items-center justify-between">
                <p className="text-sm font-medium">Items ({items.length})</p>
              </div>
              {items.map((item) => (
                <div key={item.productId} className="flex justify-between text-sm">
                  <span>{item.productName} x {item.quantity}</span>
                  <span>${(item.quantity * item.unitPrice).toFixed(2)}</span>
                </div>
              ))}
              <div className="border-t pt-2 flex justify-between font-medium">
                <span>Total ({currency})</span>
                <span>${totalAmount.toFixed(2)}</span>
              </div>
            </div>

            <div className="border rounded-md">
              <button
                type="button"
                className="w-full px-4 py-2 text-sm font-medium text-left flex items-center justify-between hover:bg-muted/50"
                onClick={() => setShowAddressFields(!showAddressFields)}
              >
                <span>Billing & Shipping Addresses (Optional)</span>
                <span className="text-muted-foreground">{showAddressFields ? "▲" : "▼"}</span>
              </button>
              {showAddressFields && (
                <div className="p-4 space-y-4">
                  <div>
                    <p className="text-sm font-medium mb-2">Billing Address</p>
                    <div className="grid gap-3 md:grid-cols-2">
                      <Input placeholder="Street" value={billingAddressStreet} onChange={(e) => setBillingAddressStreet(e.target.value)} />
                      <Input placeholder="City" value={billingAddressCity} onChange={(e) => setBillingAddressCity(e.target.value)} />
                      <Input placeholder="State" value={billingAddressState} onChange={(e) => setBillingAddressState(e.target.value)} />
                      <Input placeholder="Postal Code" value={billingAddressPostalCode} onChange={(e) => setBillingAddressPostalCode(e.target.value)} />
                      <Input placeholder="Country" value={billingAddressCountry} onChange={(e) => setBillingAddressCountry(e.target.value)} />
                    </div>
                  </div>
                  <div>
                    <p className="text-sm font-medium mb-2">Shipping Address</p>
                    <div className="grid gap-3 md:grid-cols-2">
                      <Input placeholder="Street" value={shippingAddressStreet} onChange={(e) => setShippingAddressStreet(e.target.value)} />
                      <Input placeholder="City" value={shippingAddressCity} onChange={(e) => setShippingAddressCity(e.target.value)} />
                      <Input placeholder="State" value={shippingAddressState} onChange={(e) => setShippingAddressState(e.target.value)} />
                      <Input placeholder="Postal Code" value={shippingAddressPostalCode} onChange={(e) => setShippingAddressPostalCode(e.target.value)} />
                      <Input placeholder="Country" value={shippingAddressCountry} onChange={(e) => setShippingAddressCountry(e.target.value)} />
                    </div>
                  </div>
                </div>
              )}
            </div>

            {notes && (
              <div className="p-3 border rounded-md">
                <p className="text-sm font-medium">Notes</p>
                <p className="text-sm text-muted-foreground">{notes}</p>
              </div>
            )}

            <div className="flex justify-between">
              <Button variant="outline" onClick={() => setStep(2)}>Back</Button>
              <Button onClick={handleCreateOrder} disabled={createMutation.isPending}>
                {createMutation.isPending ? "Creating..." : "Create Order"}
              </Button>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  )
}
