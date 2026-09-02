import { AdminPageHeader } from "@/components/admin/AdminPlaceholder";
import { Button } from "@/components/ui/Button";

export default function AdminNotFound() {
  return (
    <div className="admin-panel">
      <AdminPageHeader
        title="Page not found"
        description="This admin screen does not exist. Use the sidebar or return to the overview."
      />
      <p className="not-found-code mb-6" aria-hidden>
        404
      </p>
      <Button href="/admin" variant="primary" size="md">
        Back to overview
      </Button>
    </div>
  );
}
