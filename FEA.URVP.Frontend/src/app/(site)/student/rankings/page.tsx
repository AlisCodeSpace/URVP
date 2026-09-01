import { redirect } from "next/navigation";
import { studentRankingsHref } from "@/lib/auth";

/** Legacy route — rankings live on the student Ranked Projects page. */
export default function StudentRankingsRedirectPage() {
  redirect(studentRankingsHref());
}
