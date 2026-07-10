"use client"

import { useParams } from "next/navigation"
import Link from "next/link"
import { useServiceQualification } from "@/api/hooks/useServiceQualification"

export default function ServiceQualificationDetailPage() {
  const params = useParams()
  const id = params.id as string
  const { data: qualification, isLoading, error } = useServiceQualification(id)

  if (isLoading) return <div className="p-6">Loading...</div>
  if (error || !qualification) return <div className="p-6 text-red-500">Qualification not found</div>

  const resultBadge = (resultType: string) => {
    const styles: Record<string, string> = {
      Qualified: "bg-green-100 text-green-800",
      Alternate: "bg-yellow-100 text-yellow-800",
      Unqualified: "bg-red-100 text-red-800",
      UnableToDetermine: "bg-gray-100 text-gray-800",
    }
    return `px-2 py-1 rounded text-xs font-medium ${styles[resultType] || "bg-gray-100"}`
  }

  return (
    <div className="p-6 max-w-4xl mx-auto">
      <Link href="/service-qualification" className="text-blue-600 hover:underline mb-4 inline-block">
        &larr; Back to List
      </Link>

      <div className="flex justify-between items-start mb-6">
        <div>
          <h1 className="text-2xl font-bold">Qualification Result</h1>
          <p className="text-sm text-gray-500 font-mono">{qualification.id}</p>
        </div>
        <span className={`px-3 py-1 rounded text-sm font-medium ${
          qualification.state === "Done" ? "bg-green-100 text-green-800" : "bg-yellow-100 text-yellow-800"
        }`}>
          {qualification.state}
        </span>
      </div>

      <div className="grid grid-cols-2 gap-6 mb-6">
        <div className="border rounded p-4">
          <h2 className="font-semibold mb-2">Service Address</h2>
          <p>{qualification.address.street}</p>
          <p>{qualification.address.city}{qualification.address.state ? `, ${qualification.address.state}` : ""}</p>
          <p>{qualification.address.country}{qualification.address.postalCode ? ` - ${qualification.address.postalCode}` : ""}</p>
        </div>

        <div className="border rounded p-4">
          <h2 className="font-semibold mb-2">Details</h2>
          <p><span className="text-gray-600">Requested:</span> {new Date(qualification.requestedDate).toLocaleDateString()}</p>
          {qualification.expirationDate && (
            <p><span className="text-gray-600">Expires:</span> {new Date(qualification.expirationDate).toLocaleDateString()}</p>
          )}
          {qualification.description && (
            <p><span className="text-gray-600">Description:</span> {qualification.description}</p>
          )}
        </div>
      </div>

      <h2 className="text-xl font-semibold mb-4">Qualified Items</h2>
      <div className="space-y-4">
        {qualification.items.map((item) => (
          <div key={item.id} className="border rounded p-4">
            <div className="flex justify-between items-start mb-2">
              <div>
                <h3 className="font-semibold">{item.serviceName}</h3>
                <p className="text-sm text-gray-500 font-mono">{item.serviceId}</p>
              </div>
              <span className={resultBadge(item.resultType)}>{item.resultType}</span>
            </div>

            {item.eligibilityUnavailableReason && (
              <p className="text-sm text-red-600 mb-2">{item.eligibilityUnavailableReason}</p>
            )}

            <div className="grid grid-cols-2 gap-2 text-sm">
              {item.estimatedInstallDate && (
                <p><span className="text-gray-600">Est. Install:</span> {new Date(item.estimatedInstallDate).toLocaleDateString()}</p>
              )}
              {item.estimatedCompletionDate && (
                <p><span className="text-gray-600">Est. Completion:</span> {new Date(item.estimatedCompletionDate).toLocaleDateString()}</p>
              )}
            </div>

            {item.alternateProposals.length > 0 && (
              <div className="mt-3 pt-3 border-t">
                <p className="text-sm font-medium text-yellow-700 mb-2">Alternative Proposals</p>
                <div className="space-y-2">
                  {item.alternateProposals.map((alt, i) => (
                    <div key={i} className="flex justify-between items-center bg-yellow-50 px-3 py-2 rounded text-sm">
                      <span>{alt.serviceName}</span>
                      <span className={resultBadge(alt.resultType)}>{alt.resultType}</span>
                    </div>
                  ))}
                </div>
              </div>
            )}
          </div>
        ))}
      </div>
    </div>
  )
}
