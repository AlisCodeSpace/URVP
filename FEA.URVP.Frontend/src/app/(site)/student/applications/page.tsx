import { redirect } from "next/navigation";
import { studentRankingsHref } from "@/lib/auth";

/** Legacy route — rankings replaced applications. */
export default function StudentApplicationsRedirectPage() {
  redirect(studentRankingsHref());
}
