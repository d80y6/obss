import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"

interface ProductRelationship { id: string; relatedProductId: string; type: string; }
interface ProductCharacteristic { id: string; name: string; value: string; valueType: string | null; }
interface ProductPrice { id: string; priceType: string; name: string; amount: number; currency: string; recurringPeriod: number | null; recurringPeriodUnit: string | null; }
interface ProductTerm { id: string; name: string; duration: number; durationUnit: string; startDate: string; endDate: string | null; description: string | null; }
interface RealizingService { id: string; serviceId: string; serviceType: string; status: string; }
interface RealizingResource { id: string; resourceId: string; resourceType: string; status: string; }

export interface ProductDto {
  id: string;
  customerId: string;
  name: string | null;
  description: string | null;
  status: string;
  productSpecificationId: string | null;
  productOfferingId: string | null;
  activationDate: string | null;
  terminationDate: string | null;
  createdAt: string;
  updatedAt: string;
  href: string | null;
  relationships: ProductRelationship[];
  characteristics: ProductCharacteristic[];
  prices: ProductPrice[];
  terms: ProductTerm[];
  realizingServices: RealizingService[];
  realizingResources: RealizingResource[];
}

export function useProductInventoryList() {
  return useQuery<ProductDto[]>({
    queryKey: queryKeys.productInventory.lists(),
    queryFn: async () => {
      const res = await api.get("/api/v1/subscriptions/products")
      return res.data as ProductDto[]
    },
  })
}

export function useProductInventory(id: string) {
  return useQuery<ProductDto>({
    queryKey: queryKeys.productInventory.detail(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/subscriptions/products/${id}`)
      return res.data as ProductDto
    },
    enabled: !!id,
  })
}

export function useCustomerProducts(customerId: string) {
  return useQuery<ProductDto[]>({
    queryKey: [...queryKeys.productInventory.all, "customer", customerId],
    queryFn: async () => {
      const res = await api.get(`/api/v1/subscriptions/customers/${customerId}/products`)
      return res.data as ProductDto[]
    },
    enabled: !!customerId,
  })
}
