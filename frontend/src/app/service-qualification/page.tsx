"use client"

import Link from "next/link"
import { useServiceQualifications } from "@/api/hooks/useServiceQualification"

export default function ServiceQualificationsPage() {
  const { data: qualifications, isLoading, error } = useServiceQualifications()

  if (isLoading) return <div className="p-6">Loading...</div>
  if (error) return <div className="p-6 text-red-500">Error loading qualifications</div>

  return (
    <div className="p-6">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-2xl font-bold">Service Qualifications</h1>
        <Link
          href="/service-qualification/check"
          className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700"
        >
          New Qualification Check
        </Link>
      </div>

      <div className="overflow-x-auto">
        <table className="w-full border-collapse">
          <thead>
            <tr className="bg-gray-100">
              <th className="text-left p-3 border-b">ID</th>
              <th className="text-left p-3 border-b">Address</th>
              <th className="text-left p-3 border-b">State</th>
              <th className="text-left p-3 border-b">Date</th>
              <th className="text-left p-3 border-b">Result</th>
              <th className="text-left p-3 border-b">Actions</th>
            </tr>
          </thead>
          <tbody>
            {qualifications?.map((q) => (
              <tr key={q.id} className="hover:bg-gray-50 border-b">
                <td className="p-3 font-mono text-sm">{q.id.slice(0, 8)}...</td>
                <td className="p-3">{q.address.street}, {q.address.city}</td>
                <td className="p-3">
                  <span className={`px-2 py-1 rounded text-xs font-medium ${
                    q.state === "Done" ? "bg-green-100 text-green-800" :
                    q.state === "InProgress" ? "bg-yellow-100 text-yellow-800" :
                    "bg-red-100 text-red-800"
                  }`}>
                    {q.state}
                  </span>
                </td>
                <td className="p-3">{new Date(q.requestedDate).toLocaleDateString()}</td>
                <td className="p-3">
                  {q.items.every(i => i.resultType === "Qualified") ? (
                    <span className="text-green-600 font-medium">All Qualified</span>
                  ) : q.items.some(i => i.resultType === "Qualified") ? (
                    <span className="text-yellow-600 font-medium">Partial</span>
                  ) : (
                    <span className="text-red-600 font-medium">Not Qualified</span>
                  )}
                </td>
                <td className="p-3">
                  <Link
                    href={`/service-qualification/${q.id}`}
                    className="text-blue-600 hover:underline"
                  >
                    View Details
                  </Link>
                </td>
              </tr>
            ))}
            {(!qualifications || qualifications.length === 0) && (
              <tr>
                <td colSpan={6} className="p-6 text-center text-gray-500">
                  No qualifications found.{" "}
                  <Link href="/service-qualification/check" className="text-blue-600 hover:underline">
                    Check a service
                  </Link>
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  )
}
