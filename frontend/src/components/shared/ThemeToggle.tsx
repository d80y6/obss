"use client"

import { Moon, Sun } from "lucide-react"
import { Button } from "@/components/ui/button"
import { useThemeStore } from "@/stores/theme-store"

export function ThemeToggle() {
  const { theme, toggleTheme } = useThemeStore()

  return (
    <Button variant="ghost" size="icon" onClick={toggleTheme} title={`Switch to ${theme === "light" ? "dark" : "light"} mode`}>
      {theme === "light" ? <Moon className="h-4 w-4" /> : <Sun className="h-4 w-4" />}
    </Button>
  )
}
