import { redirect } from "next/navigation";
import { studentProfileHref } from "@/lib/auth";

export default function StudentPortalIndexPage() {
  redirect(studentProfileHref());
}
