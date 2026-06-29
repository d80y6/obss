import { create } from "zustand"
import { persist } from "zustand/middleware"
import { IamUserDto } from "@/types/api"

interface AuthState {
  token: string | null
  refreshToken: string | null
  user: IamUserDto | null
  isAuthenticated: boolean
  setAuth: (token: string, refreshToken: string, user: IamUserDto) => void
  setUser: (user: IamUserDto) => void
  logout: () => void
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      token: null,
      refreshToken: null,
      user: null,
      isAuthenticated: false,
      setAuth: (token, refreshToken, user) =>
        set({ token, refreshToken, user, isAuthenticated: true }),
      setUser: (user) => set({ user }),
      logout: () =>
        set({
          token: null,
          refreshToken: null,
          user: null,
          isAuthenticated: false,
        }),
    }),
    {
      name: "auth-storage",
    }
  )
)
