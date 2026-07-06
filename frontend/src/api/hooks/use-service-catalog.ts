import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"

// --- Inline types (no generated types yet) ---
export interface ServiceCategoryDto {
  id: string
  tenantId: string
  name: string
  description?: string
  parentCategoryId?: string
  lifecycleStatus: string
  version: number
  validFrom?: string
  validTo?: string
  createdAt: string
  updatedAt: string
  isRoot: boolean
}

export interface ServiceCandidateDto {
  id: string
  tenantId: string
  name: string
  description?: string
  lifecycleStatus: string
  version: number
  validFrom?: string
  validTo?: string
  serviceSpecificationId?: string
  serviceSpecificationName?: string
  baseCandidateId?: string
  featureSpecification?: string
  createdAt: string
  updatedAt: string
  categories: ServiceCategoryDto[]
}

export interface ServiceSpecificationDto {
  id: string
  tenantId: string
  name: string
  description?: string
  brand?: string
  version?: string
  lifecycleStatus: string
  isBundle: boolean
  validFrom?: string
  validTo?: string
  createdAt: string
  updatedAt: string
  characteristics: ServiceSpecCharacteristicDto[]
  relationships: ServiceSpecRelationshipDto[]
}

export interface ServiceSpecCharacteristicDto {
  id: string
  serviceSpecificationId: string
  name: string
  description?: string
  valueType: string
  configurable: boolean
  minValue?: number
  maxValue?: number
  regex?: string
  sortOrder: number
  maxCardinality?: number
  isRequired: boolean
  values: ServiceSpecCharValueDto[]
}

export interface ServiceSpecCharValueDto {
  id: string
  characteristicId: string
  value: string
  unitOfMeasure?: string
  isDefault: boolean
  valueFrom?: string
  valueTo?: string
  rangeInterval?: string
  validFrom?: string
  validTo?: string
}

export interface ServiceSpecRelationshipDto {
  id: string
  serviceSpecificationId: string
  targetSpecificationId: string
  relationshipType: string
  role?: string
  validFrom?: string
  validTo?: string
}

// --- Categories ---
export function useServiceCategories(filters: Record<string, string> = {}) {
  return useQuery({
    queryKey: queryKeys.serviceCatalog.categories.list(filters),
    queryFn: async () => {
      const params = new URLSearchParams()
      Object.entries(filters).forEach(([k, v]) => { if (v) params.set(k, v) })
      const res = await api.get(`/api/v1/service-catalog/service-categories?${params.toString()}`)
      const total = res.headers['x-total-count'] ? parseInt(res.headers['x-total-count'], 10) : null
      return { items: res.data as ServiceCategoryDto[], total }
    },
  })
}

export function useServiceCategory(id: string) {
  return useQuery({
    queryKey: queryKeys.serviceCatalog.categories.detail(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/service-catalog/service-categories/${id}`)
      return res.data as ServiceCategoryDto
    },
    enabled: !!id,
  })
}

export function useCreateServiceCategory() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: Record<string, unknown>) => {
      const res = await api.post("/api/v1/service-catalog/service-categories", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.serviceCatalog.categories.all })
    },
  })
}

export function useUpdateServiceCategory() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ id, ...data }: { id: string } & Record<string, unknown>) => {
      const res = await api.patch(`/api/v1/service-catalog/service-categories/${id}`, data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.serviceCatalog.categories.all })
    },
  })
}

export function useDeleteServiceCategory() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      await api.delete(`/api/v1/service-catalog/service-categories/${id}`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.serviceCatalog.categories.all })
    },
  })
}

// --- Candidates ---
export function useServiceCandidates(filters: Record<string, string> = {}) {
  return useQuery({
    queryKey: queryKeys.serviceCatalog.candidates.list(filters),
    queryFn: async () => {
      const params = new URLSearchParams()
      Object.entries(filters).forEach(([k, v]) => { if (v) params.set(k, v) })
      const res = await api.get(`/api/v1/service-catalog/service-candidates?${params.toString()}`)
      const total = res.headers['x-total-count'] ? parseInt(res.headers['x-total-count'], 10) : null
      return { items: res.data as ServiceCandidateDto[], total }
    },
  })
}

export function useServiceCandidate(id: string) {
  return useQuery({
    queryKey: queryKeys.serviceCatalog.candidates.detail(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/service-catalog/service-candidates/${id}`)
      return res.data as ServiceCandidateDto
    },
    enabled: !!id,
  })
}

export function useCreateServiceCandidate() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: Record<string, unknown>) => {
      const res = await api.post("/api/v1/service-catalog/service-candidates", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.serviceCatalog.candidates.all })
    },
  })
}

export function useUpdateServiceCandidate() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ id, ...data }: { id: string } & Record<string, unknown>) => {
      const res = await api.patch(`/api/v1/service-catalog/service-candidates/${id}`, data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.serviceCatalog.candidates.all })
    },
  })
}

export function useDeleteServiceCandidate() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      await api.delete(`/api/v1/service-catalog/service-candidates/${id}`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.serviceCatalog.candidates.all })
    },
  })
}

// --- Specifications ---
export function useServiceSpecifications(filters: Record<string, string> = {}) {
  return useQuery({
    queryKey: queryKeys.serviceCatalog.specifications.list(filters),
    queryFn: async () => {
      const params = new URLSearchParams()
      Object.entries(filters).forEach(([k, v]) => { if (v) params.set(k, v) })
      const res = await api.get(`/api/v1/service-catalog/service-specifications?${params.toString()}`)
      const total = res.headers['x-total-count'] ? parseInt(res.headers['x-total-count'], 10) : null
      return { items: res.data as ServiceSpecificationDto[], total }
    },
  })
}

export function useServiceSpecification(id: string) {
  return useQuery({
    queryKey: queryKeys.serviceCatalog.specifications.detail(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/service-catalog/service-specifications/${id}`)
      return res.data as ServiceSpecificationDto
    },
    enabled: !!id,
  })
}

export function useCreateServiceSpecification() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: Record<string, unknown>) => {
      const res = await api.post("/api/v1/service-catalog/service-specifications", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.serviceCatalog.specifications.all })
    },
  })
}

export function useUpdateServiceSpecification() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ id, ...data }: { id: string } & Record<string, unknown>) => {
      const res = await api.patch(`/api/v1/service-catalog/service-specifications/${id}`, data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.serviceCatalog.specifications.all })
    },
  })
}

export function useDeleteServiceSpecification() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      await api.delete(`/api/v1/service-catalog/service-specifications/${id}`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.serviceCatalog.specifications.all })
    },
  })
}

// --- Characteristics ---
export function useServiceCharacteristics(specId: string) {
  return useQuery({
    queryKey: queryKeys.serviceCatalog.characteristics.list(specId),
    queryFn: async () => {
      const res = await api.get(`/api/v1/service-catalog/service-specifications/${specId}/characteristics`)
      return res.data as ServiceSpecCharacteristicDto[]
    },
    enabled: !!specId,
  })
}

export function useAddCharacteristic() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ specId, ...data }: { specId: string } & Record<string, unknown>) => {
      const res = await api.post(`/api/v1/service-catalog/service-specifications/${specId}/characteristics`, data)
      return res.data
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.serviceCatalog.characteristics.all(variables.specId) })
    },
  })
}

// --- Characteristic Values ---
export function useServiceCharacteristicValues(specId: string, charId: string) {
  return useQuery({
    queryKey: queryKeys.serviceCatalog.characteristicValues.list(specId, charId),
    queryFn: async () => {
      const res = await api.get(`/api/v1/service-catalog/service-specifications/${specId}/characteristics/${charId}/values`)
      return res.data as ServiceSpecCharValueDto[]
    },
    enabled: !!specId && !!charId,
  })
}

// --- Relationships ---
export function useServiceSpecRelationships(specId: string) {
  return useQuery({
    queryKey: queryKeys.serviceCatalog.relationships.list(specId),
    queryFn: async () => {
      const res = await api.get(`/api/v1/service-catalog/service-specifications/${specId}/relationships`)
      return res.data as ServiceSpecRelationshipDto[]
    },
    enabled: !!specId,
  })
}
