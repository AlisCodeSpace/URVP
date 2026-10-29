"use client";

import { useEffect } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { RequireAuth } from "@/components/auth/RequireAuth";
import { useAuth } from "@/components/auth/AuthProvider";
import { useApplicationWindow } from "@/hooks/useApplicationWindow";
import { AdminAssignStudentsView } from "@/components/admin/AdminAssignStudentsView";
import { AdminMatchingRunDetailView } from "@/components/admin/AdminMatchingRunDetailView";
import { AdminNewsForm } from "@/components/admin/AdminNewsForm";
import { AdminProjectDetailView } from "@/components/admin/AdminProjectDetailView";
import { AdminStudentProfileView } from "@/components/admin/AdminStudentProfileView";
import { AdminSemesterForm } from "@/components/admin/AdminSemesterForm";
import { AdminWorkshopForm } from "@/components/admin/AdminWorkshopForm";
import { PageHeader } from "@/components/layout/PageHeader";
import { NewsArticleLoader } from "@/components/news/NewsArticleLoader";
import { EditProjectView } from "@/components/projects/EditProjectView";
import { FacultyProjectView } from "@/components/projects/FacultyProjectView";
import { MyProjectsList } from "@/components/projects/MyProjectsList";
import { PostProjectForm } from "@/components/projects/PostProjectForm";
import { ProjectDetailLoader } from "@/components/projects/ProjectDetailLoader";
import { FacultyStudentProfileView } from "@/components/student/FacultyStudentProfileView";
import { Button } from "@/components/ui/Button";
import { IconPlus } from "@/components/ui/Icons";
import { NotFoundView } from "@/components/ui/NotFoundView";
import { PageLoader } from "@/components/ui/PageLoader";
import Link from "next/link";
import {
  FACULTY_PORTAL_ROLES,
  myProjectsHref,
  newProjectHref,
  portalHref,
  RouteParam,
} from "@/lib/auth";

/**
 * Client entry points for routes whose identifier lives in the query string.
 *
 * `output: 'export'` prerenders one HTML file per route, so identifiers cannot be path segments
 * (see the note on `RouteParam` in `src/lib/auth.ts`). Each wrapper below reads its parameters on
 * the client and hands them to the existing view component unchanged.
 *
 * They are collected in one module so the `useSearchParams` plumbing exists once. Every wrapper is
 * rendered inside a `<Suspense>` boundary by its page, which `useSearchParams` requires under
 * static export.
 */

/** Reads a query-string route parameter. */
function useRouteParam(name: string): string {
  return useSearchParams().get(name)?.trim() ?? "";
}

const GUID = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

/**
 * Reads an identifier parameter, or "" when it is not a GUID. Identifiers are interpolated into
 * API paths, so anything else (such as `../`) must never reach a request.
 */
function useIdParam(name: string): string {
  const value = useRouteParam(name);
  return GUID.test(value) ? value : "";
}

/** Shown when a link or a hand-typed URL omits a required identifier. */
function MissingParam() {
  return <NotFoundView />;
}

/* -------------------------------------------------------------------------- */
/* Faculty portal                                                             */
/* -------------------------------------------------------------------------- */

/**
 * `/my-projects` — the faculty project list when `?user=` is present, otherwise a role-based
 * redirect to the caller's own portal.
 */
export function MyProjectsRoute() {
  const router = useRouter();
  const userId = useIdParam(RouteParam.User);
  const { status, loading } = useAuth();
  const phase = useApplicationWindow();
  const canPost = !phase.loading && phase.registrationOpen;

  useEffect(() => {
    if (userId || loading) return;

    if (!status?.isAuthenticated || !status.userId) {
      router.replace("/sign-in");
      return;
    }

    router.replace(portalHref(status.role, status.userId));
  }, [userId, loading, status, router]);

  if (!userId) {
    return <PageLoader label="Redirecting" />;
  }

  return (
    <RequireAuth userId={userId} roles={FACULTY_PORTAL_ROLES}>
      <main className="flex-1 bg-background">
        <PageHeader
          eyebrow="Faculty portal"
          title="My projects"
          description="Review projects you have posted and open new opportunities for undergraduate volunteers."
          actions={
            canPost ? (
              <Button
                href={newProjectHref(userId)}
                variant="secondary"
                size="lg"
                data-tour="faculty-new-project"
              >
                <IconPlus />
                New project
              </Button>
            ) : (
              <Button
                type="button"
                variant="secondary"
                size="lg"
                disabled
                title="Projects can be posted only while the registration window is open."
              >
                <IconPlus />
                New project
              </Button>
            )
          }
        />

        <section
          className="site-container py-14 sm:py-16"
          data-tour="faculty-projects"
        >
          <MyProjectsList userId={userId} />
        </section>
      </main>
    </RequireAuth>
  );
}

/** `/my-projects/new?user=` */
export function NewProjectRoute() {
  const userId = useIdParam(RouteParam.User);

  if (!userId) return <MissingParam />;

  return (
    <RequireAuth userId={userId} roles={FACULTY_PORTAL_ROLES}>
      <main className="flex-1 bg-background">
        <PageHeader
          eyebrow="New listing"
          title="Post a project"
          description="Share a research opportunity across AUB faculties, centers, and institutes for undergraduate matching."
        >
          <Link
            href={myProjectsHref(userId)}
            className="inline-flex items-center gap-2 text-sm text-primary/70 transition hover:text-primary"
          >
            <span aria-hidden>←</span>
            Back to my projects
          </Link>
        </PageHeader>

        <section className="site-container py-14 sm:py-16">
          <PostProjectForm userId={userId} />
        </section>
      </main>
    </RequireAuth>
  );
}

/** `/my-projects/project?user=&project=` */
export function FacultyProjectRoute() {
  const userId = useIdParam(RouteParam.User);
  const projectId = useIdParam(RouteParam.Project);

  if (!userId || !projectId) return <MissingParam />;

  return <FacultyProjectView userId={userId} projectId={projectId} />;
}

/** `/my-projects/project/edit?user=&project=` */
export function EditProjectRoute() {
  const userId = useIdParam(RouteParam.User);
  const projectId = useIdParam(RouteParam.Project);

  if (!userId || !projectId) return <MissingParam />;

  return <EditProjectView userId={userId} projectId={projectId} />;
}

/** `/my-projects/project/student?user=&project=&student=` */
export function FacultyStudentProfileRoute() {
  const userId = useIdParam(RouteParam.User);
  const projectId = useIdParam(RouteParam.Project);
  const studentUserId = useIdParam(RouteParam.Student);

  if (!userId || !projectId || !studentUserId) return <MissingParam />;

  return (
    <FacultyStudentProfileView
      userId={userId}
      projectId={projectId}
      studentUserId={studentUserId}
    />
  );
}

/** `/research-assistant/project?project=` */
export function AssistedProjectRoute() {
  const projectId = useIdParam(RouteParam.Project);

  if (!projectId) return <MissingParam />;

  return <FacultyProjectView projectId={projectId} mode="assistant" />;
}

/** `/research-assistant/project/student?project=&student=` */
export function AssistedStudentRoute() {
  const projectId = useIdParam(RouteParam.Project);
  const studentUserId = useIdParam(RouteParam.Student);

  if (!projectId || !studentUserId) return <MissingParam />;

  return (
    <FacultyStudentProfileView
      projectId={projectId}
      studentUserId={studentUserId}
      mode="assistant"
    />
  );
}

/* -------------------------------------------------------------------------- */
/* Public site                                                                */
/* -------------------------------------------------------------------------- */

/** `/projects/detail?id=` */
export function ProjectDetailRoute() {
  const projectId = useIdParam(RouteParam.Id);

  if (!projectId) return <MissingParam />;

  return (
    <RequireAuth>
      <main className="flex-1 bg-background">
        <ProjectDetailLoader id={projectId} />
      </main>
    </RequireAuth>
  );
}

/** `/news/article?slug=` */
export function NewsArticleRoute() {
  const slug = useRouteParam(RouteParam.Slug);

  if (!slug) return <MissingParam />;

  return <NewsArticleLoader slug={slug} />;
}

/* -------------------------------------------------------------------------- */
/* Admin console                                                              */
/* -------------------------------------------------------------------------- */

/** `/admin/projects/detail?id=` */
export function AdminProjectRoute() {
  const projectId = useIdParam(RouteParam.Id);

  if (!projectId) return <MissingParam />;

  return <AdminProjectDetailView projectId={projectId} />;
}

/** `/admin/projects/assign?id=` */
export function AdminAssignStudentsRoute() {
  const projectId = useIdParam(RouteParam.Id);

  if (!projectId) return <MissingParam />;

  return <AdminAssignStudentsView projectId={projectId} />;
}

/** `/admin/projects/student?id=&student=` */
export function AdminStudentProfileRoute() {
  const projectId = useIdParam(RouteParam.Id);
  const studentUserId = useIdParam(RouteParam.Student);

  if (!projectId || !studentUserId) return <MissingParam />;

  return (
    <AdminStudentProfileView
      projectId={projectId}
      studentUserId={studentUserId}
    />
  );
}

/** `/admin/users/student?student=` */
export function AdminUserStudentProfileRoute() {
  const studentUserId = useIdParam(RouteParam.Student);

  if (!studentUserId) return <MissingParam />;

  return <AdminStudentProfileView studentUserId={studentUserId} />;
}

/** `/admin/matching/run?id=` */
export function AdminMatchingRunRoute() {
  const runId = useIdParam(RouteParam.Id);

  if (!runId) return <MissingParam />;

  return <AdminMatchingRunDetailView runId={runId} />;
}

/** `/admin/news/edit?id=` */
export function AdminNewsEditRoute() {
  const newsId = useIdParam(RouteParam.Id);

  if (!newsId) return <MissingParam />;

  return <AdminNewsForm newsId={newsId} />;
}

/** `/admin/semesters/edit?id=` */
export function AdminSemesterEditRoute() {
  const semesterId = useIdParam(RouteParam.Id);

  if (!semesterId) return <MissingParam />;

  return <AdminSemesterForm semesterId={semesterId} />;
}

/** `/admin/workshops/edit?id=` */
export function AdminWorkshopEditRoute() {
  const workshopId = useIdParam(RouteParam.Id);

  if (!workshopId) return <MissingParam />;

  return <AdminWorkshopForm workshopId={workshopId} />;
}
