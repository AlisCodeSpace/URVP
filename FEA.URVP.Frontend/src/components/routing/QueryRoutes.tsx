"use client";

import { useEffect } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { RequireAuth } from "@/components/auth/RequireAuth";
import { useAuth } from "@/components/auth/AuthProvider";
import { AdminMatchingRunDetailView } from "@/components/admin/AdminMatchingRunDetailView";
import { AdminNewsForm } from "@/components/admin/AdminNewsForm";
import { AdminProjectDetailView } from "@/components/admin/AdminProjectDetailView";
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
  const userId = useRouteParam(RouteParam.User);
  const { status, loading } = useAuth();

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
            <Button href={newProjectHref(userId)} variant="secondary" size="lg">
              <IconPlus />
              New project
            </Button>
          }
        />

        <section className="site-container py-14 sm:py-16">
          <MyProjectsList userId={userId} />
        </section>
      </main>
    </RequireAuth>
  );
}

/** `/my-projects/new?user=` */
export function NewProjectRoute() {
  const userId = useRouteParam(RouteParam.User);

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
            className="inline-flex items-center gap-2 text-sm text-white/65 transition hover:text-secondary"
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
  const userId = useRouteParam(RouteParam.User);
  const projectId = useRouteParam(RouteParam.Project);

  if (!userId || !projectId) return <MissingParam />;

  return <FacultyProjectView userId={userId} projectId={projectId} />;
}

/** `/my-projects/project/edit?user=&project=` */
export function EditProjectRoute() {
  const userId = useRouteParam(RouteParam.User);
  const projectId = useRouteParam(RouteParam.Project);

  if (!userId || !projectId) return <MissingParam />;

  return <EditProjectView userId={userId} projectId={projectId} />;
}

/** `/my-projects/project/student?user=&project=&student=` */
export function FacultyStudentProfileRoute() {
  const userId = useRouteParam(RouteParam.User);
  const projectId = useRouteParam(RouteParam.Project);
  const studentUserId = useRouteParam(RouteParam.Student);

  if (!userId || !projectId || !studentUserId) return <MissingParam />;

  return (
    <FacultyStudentProfileView
      userId={userId}
      projectId={projectId}
      studentUserId={studentUserId}
    />
  );
}

/* -------------------------------------------------------------------------- */
/* Public site                                                                */
/* -------------------------------------------------------------------------- */

/** `/projects/detail?id=` */
export function ProjectDetailRoute() {
  const projectId = useRouteParam(RouteParam.Id);

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
  const projectId = useRouteParam(RouteParam.Id);

  if (!projectId) return <MissingParam />;

  return <AdminProjectDetailView projectId={projectId} />;
}

/** `/admin/matching/run?id=` */
export function AdminMatchingRunRoute() {
  const runId = useRouteParam(RouteParam.Id);

  if (!runId) return <MissingParam />;

  return <AdminMatchingRunDetailView runId={runId} />;
}

/** `/admin/news/edit?id=` */
export function AdminNewsEditRoute() {
  const newsId = useRouteParam(RouteParam.Id);

  if (!newsId) return <MissingParam />;

  return <AdminNewsForm newsId={newsId} />;
}

/** `/admin/semesters/edit?id=` */
export function AdminSemesterEditRoute() {
  const semesterId = useRouteParam(RouteParam.Id);

  if (!semesterId) return <MissingParam />;

  return <AdminSemesterForm semesterId={semesterId} />;
}

/** `/admin/workshops/edit?id=` */
export function AdminWorkshopEditRoute() {
  const workshopId = useRouteParam(RouteParam.Id);

  if (!workshopId) return <MissingParam />;

  return <AdminWorkshopForm workshopId={workshopId} />;
}
