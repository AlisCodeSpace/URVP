import { ClientRedirect } from "@/components/routing/ClientRedirect";
import { studentRankingsHref } from "@/lib/auth";

/** Legacy route — rankings live on the student Ranked Projects page. */
export default function StudentRankingsRedirectPage() {
  return <ClientRedirect href={studentRankingsHref()} />;
}
