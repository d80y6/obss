"use client"

import { Button } from "@/components/ui/button"
import { useLocaleStore } from "@/stores/locale-store"

export function LocaleToggle() {
  const { locale, toggleLocale } = useLocaleStore()

  return (
    <Button variant="ghost" size="sm" onClick={toggleLocale}>
      {locale === "en" ? "العربية" : "English"}
    </Button>
  )
}
