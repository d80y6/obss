import { create } from "zustand"
import { persist } from "zustand/middleware"

export type Locale = "en" | "ar"

interface LocaleState {
  locale: Locale
  dir: "ltr" | "rtl"
  setLocale: (locale: Locale) => void
  toggleLocale: () => void
}

export const useLocaleStore = create<LocaleState>()(
  persist(
    (set) => ({
      locale: "en",
      dir: "ltr",
      setLocale: (locale) =>
        set({
          locale,
          dir: locale === "ar" ? "rtl" : "ltr",
        }),
      toggleLocale: () =>
        set((state) => ({
          locale: state.locale === "en" ? "ar" : "en",
          dir: state.locale === "en" ? "rtl" : "ltr",
        })),
    }),
    { name: "locale-storage" }
  )
)
