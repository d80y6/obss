import axios, { AxiosError, InternalAxiosRequestConfig } from "axios"

const api = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_URL || "http://localhost:5020",
  timeout: 30000,
  headers: {
    "Content-Type": "application/json",
  },
})

api.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    if (typeof window !== "undefined") {
      const token = localStorage.getItem("auth-token")
      if (token) {
        config.headers.Authorization = `Bearer ${token}`
      }
      let tenantId = ""
      const stored = localStorage.getItem("tenant-storage")
      if (stored) {
        try {
          const { state } = JSON.parse(stored)
          if (state?.tenant?.id) {
            tenantId = state.tenant.id
            config.headers["X-Tenant-Id"] = tenantId
          }
        } catch {}
      }
      if (tenantId && config.data && typeof config.data === "object" && !(config.data instanceof FormData)) {
        const body = config.data as Record<string, unknown>
        if (!("tenantId" in body)) {
          config.data = { ...body, tenantId }
        }
      }
    }
    return config
  },
  (error: AxiosError) => {
    return Promise.reject(error)
  }
)

api.interceptors.response.use(
  (response) => response,
  (error: AxiosError) => {
    if (error.response?.status === 401) {
      if (typeof window !== "undefined") {
        localStorage.removeItem("auth-token")
        window.location.href = "/login"
      }
    }
    return Promise.reject(error)
  }
)

export async function login(username: string, password: string) {
  const formData = new URLSearchParams()
  formData.append("client_id", process.env.NEXT_PUBLIC_KEYCLOAK_CLIENT_ID || "obss-frontend")
  formData.append("username", username)
  formData.append("password", password)
  formData.append("grant_type", "password")
  const res = await axios.post<{ access_token: string; refresh_token: string; expires_in: number }>(
    `${process.env.NEXT_PUBLIC_KEYCLOAK_URL}/realms/${process.env.NEXT_PUBLIC_KEYCLOAK_REALM}/protocol/openid-connect/token`,
    formData.toString(),
    { headers: { "Content-Type": "application/x-www-form-urlencoded" } }
  )
  return res.data
}

export async function refreshToken(token: string) {
  const formData = new URLSearchParams()
  formData.append("client_id", process.env.NEXT_PUBLIC_KEYCLOAK_CLIENT_ID || "obss-frontend")
  formData.append("refresh_token", token)
  formData.append("grant_type", "refresh_token")
  const res = await axios.post<{ access_token: string; refresh_token: string; expires_in: number }>(
    `${process.env.NEXT_PUBLIC_KEYCLOAK_URL}/realms/${process.env.NEXT_PUBLIC_KEYCLOAK_REALM}/protocol/openid-connect/token`,
    formData.toString(),
    { headers: { "Content-Type": "application/x-www-form-urlencoded" } }
  )
  return res.data
}

export function getToken(): string | null {
  if (typeof window !== "undefined") {
    return localStorage.getItem("auth-token")
  }
  return null
}

export default api
