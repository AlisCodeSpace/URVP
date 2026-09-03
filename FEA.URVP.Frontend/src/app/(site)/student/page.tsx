import { ClientRedirect } from "@/components/routing/ClientRedirect";
import { studentProfileHref } from "@/lib/auth";

export default function StudentPortalIndexPage() {
  return <ClientRedirect href={studentProfileHref()} />;
}
