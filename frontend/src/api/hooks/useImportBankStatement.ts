import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"

interface BankTransactionLine {
  externalReference: string
  amount: number
  transactionDate: string
  description?: string | null
}

interface ImportBankStatementCommand {
  importSource: string
  importFileName?: string | null
  currency: string
  transactions: BankTransactionLine[]
}

export function useImportBankStatement() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: ImportBankStatementCommand) => {
      const res = await api.post("/api/v1/payments/payments/reconciliation/import", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.payments.reconciliation.all })
      queryClient.invalidateQueries({ queryKey: queryKeys.payments.unmatched })
    },
  })
}
