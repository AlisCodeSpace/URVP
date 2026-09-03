import { ClientRedirect } from "@/components/routing/ClientRedirect";
import { studentRankingsHref } from "@/lib/auth";

/** Legacy route — rankings replaced applications. */
export default function StudentApplicationsRedirectPage() {
  return <ClientRedirect href={studentRankingsHref()} />;
}
