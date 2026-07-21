import { getErrorMessage } from "@/lib/api-helpers"

const API_BASE = process.env.NEXT_PUBLIC_API_URL || "/api/v1"

export interface Service {
  id: string
  name: string
  type: string
  status: "ACTIVE" | "PENDING" | "SUSPENDED" | "CANCELLED"
  plan: string
  nextBillingDate: string
  startDate: string
  monthlyFee: number
  usage?: {
    dataUsed: number
    dataIncluded: number
    voiceUsed: number
    voiceIncluded: number
    smsUsed: number
    smsIncluded: number
  }
}

export interface Bill {
  id: string
  billNumber: string
  period: string
  amount: number
  status: "PAID" | "PENDING" | "OVERDUE"
  dueDate: string
  paidAt?: string
  pdfUrl?: string
}

export interface BillLineItem {
  description: string
  amount: number
  type: "CHARGE" | "TAX" | "DISCOUNT" | "CREDIT"
}

export interface BillDetail extends Bill {
  lineItems: BillLineItem[]
  subtotal: number
  taxTotal: number
  discountTotal: number
  total: number
  payments: Payment[]
}

export interface Payment {
  id: string
  amount: number
  method: string
  status: string
  paidAt: string
  reference: string
}

export interface Ticket {
  id: string
  ticketNumber: string
  subject: string
  status: "OPEN" | "IN_PROGRESS" | "RESOLVED" | "CLOSED"
  priority: "LOW" | "MEDIUM" | "HIGH"
  category: string
  createdAt: string
  updatedAt: string
}

export interface TicketMessage {
  id: string
  content: string
  sender: "CUSTOMER" | "AGENT"
  senderName: string
  createdAt: string
}

export interface TicketDetail extends Ticket {
  messages: TicketMessage[]
}

export interface CustomerProfile {
  id: string
  fullName: string
  email: string
  phone: string
  address: string
  notificationPreferences: {
    email: boolean
    sms: boolean
    marketing: boolean
  }
}

export interface CreateTicketData {
  subject: string
  category: string
  priority: string
  description: string
}

export interface UpdateProfileData {
  fullName?: string
  email?: string
  phone?: string
  address?: string
}

async function apiFetch<T>(url: string, options?: RequestInit): Promise<T> {
  const res = await fetch(`${API_BASE}${url}`, {
    headers: { "Content-Type": "application/json", ...options?.headers },
    ...options,
  })
  if (!res.ok) {
    const errorBody = await res.json().catch(() => null)
    throw new Error(errorBody?.message || `Request failed with status ${res.status}`)
  }
  return res.json()
}

export function fetchServices(): Promise<Service[]> {
  return apiFetch<Service[]>("/subscriptions")
}

export function fetchServiceDetail(id: string): Promise<Service> {
  return apiFetch<Service>(`/subscriptions/${id}`)
}

export function fetchBills(): Promise<Bill[]> {
  return apiFetch<Bill[]>("/invoices")
}

export function fetchBillDetail(id: string): Promise<BillDetail> {
  return apiFetch<BillDetail>(`/invoices/${id}`)
}

export function fetchTickets(): Promise<Ticket[]> {
  return apiFetch<Ticket[]>("/tickets")
}

export function createTicket(data: CreateTicketData): Promise<Ticket> {
  return apiFetch<Ticket>("/tickets", {
    method: "POST",
    body: JSON.stringify(data),
  })
}

export function fetchTicketDetail(id: string): Promise<TicketDetail> {
  return apiFetch<TicketDetail>(`/tickets/${id}`)
}

export function replyToTicket(id: string, content: string): Promise<TicketMessage> {
  return apiFetch<TicketMessage>(`/tickets/${id}/messages`, {
    method: "POST",
    body: JSON.stringify({ content }),
  })
}

export function fetchProfile(): Promise<CustomerProfile> {
  return apiFetch<CustomerProfile>("/customers/profile")
}

export function updateProfile(data: UpdateProfileData): Promise<CustomerProfile> {
  return apiFetch<CustomerProfile>("/customers/profile", {
    method: "PUT",
    body: JSON.stringify(data),
  })
}
