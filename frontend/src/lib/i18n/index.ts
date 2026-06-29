import { en } from "./en"
import { ar } from "./ar"
import { useLocaleStore } from "@/stores/locale-store"

export type TranslationKey = keyof typeof en

const translations = { en, ar } as const

function getNestedValue(obj: Record<string, unknown>, path: string): string {
  const keys = path.split(".")
  let current: unknown = obj
  for (const key of keys) {
    if (current && typeof current === "object" && key in (current as Record<string, unknown>)) {
      current = (current as Record<string, unknown>)[key]
    } else {
      return path
    }
  }
  return typeof current === "string" ? current : path
}

export function t(path: string, params?: Record<string, string | number>): string {
  const { locale } = useLocaleStore.getState()
  const lang = translations[locale as keyof typeof translations] || translations.en
  let value = getNestedValue(lang as unknown as Record<string, unknown>, path)
  if (params) {
    Object.entries(params).forEach(([key, val]) => {
      value = value.replace(`{${key}}`, String(val))
    })
  }
  return value
}

export function useTranslation() {
  const { locale, dir, setLocale, toggleLocale } = useLocaleStore()

  const _t = (path: string, params?: Record<string, string | number>): string => {
    return t(path, params)
  }

  return { t: _t, locale, dir, setLocale, toggleLocale }
}
