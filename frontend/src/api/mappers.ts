import type { CreateCustomerCommand, CustomerDto, CreateOrderCommand } from '@/api/generated'

// ── Customer Mappers ──

export interface CustomerFormData {
  firstName: string
  lastName: string
  email: string
  phone: string
  address: string
  city: string
  state: string
  zipCode: string
  country: string
}

export function toCreateCustomerCommand(form: CustomerFormData): CreateCustomerCommand {
  return {
    tenantId: '',
    customerType: 'Residential',
    companyName: null,
    displayName: `${form.firstName} ${form.lastName}`.trim(),
    taxNumber: null,
    registrationNumber: null,
    email: form.email,
    phoneNumber: form.phone || null,
    countryCode: form.country || null,
    website: null,
    currency: 'USD',
  }
}

export function customerDtoToForm(dto: CustomerDto): CustomerFormData {
  const nameParts = (dto.displayName || '').split(' ')
  return {
    firstName: nameParts[0] || '',
    lastName: nameParts.slice(1).join(' ') || '',
    email: dto.email || '',
    phone: dto.phoneNumber || '',
    address: '',
    city: '',
    state: '',
    zipCode: '',
    country: dto.countryCode || '',
  }
}

// ── Order Mappers ──

export interface OrderFormItem {
  productId: string
  productName: string
  offerId: string
  offerName: string
  quantity: number
  unitPrice: number
}

export interface OrderFormData {
  customerId: string
  customerName: string
  items: OrderFormItem[]
  notes?: string
}

export function toCreateOrderCommand(form: OrderFormData): CreateOrderCommand {
  return {
    customerId: form.customerId,
    customerName: form.customerName,
    orderType: 'STANDARD',
    notes: form.notes || null,
    billingAddressStreet: null,
    billingAddressCity: null,
    billingAddressState: null,
    billingAddressPostalCode: null,
    billingAddressCountry: null,
    shippingAddressStreet: null,
    shippingAddressCity: null,
    shippingAddressState: null,
    shippingAddressPostalCode: null,
    shippingAddressCountry: null,
    currency: 'USD',
    items: form.items.map(item => ({
      productId: item.productId,
      offerId: item.offerId,
      productName: item.productName,
      offerName: item.offerName,
      quantity: item.quantity,
      unitPrice: item.unitPrice,
      recurringPrice: 0,
      discountAmount: 0,
      taxAmount: 0,
      billingPeriod: 'Monthly',
    })),
  }
}

// ── Payment Mappers ──

export interface PaymentFormData {
  customerId: string
  invoiceId: string
  amount: number
  method: string
  reference?: string
  paidAt: string
}

export function toRecordPaymentCommand(form: PaymentFormData) {
  return {
    customerId: form.customerId,
    invoiceId: form.invoiceId || null,
    amount: form.amount,
    paymentMethod: form.method,
    paymentReference: form.reference || null,
    currency: 'USD',
    notes: null,
  }
}
