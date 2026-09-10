import type { Step } from "react-joyride";
import {
  myProjectsHref,
  newProjectHref,
  projectsHref,
  studentProfileHref,
  studentRankingsHref,
} from "@/lib/auth";
import { waitForTourTarget } from "@/lib/tour";

type Go = (href: string) => Promise<void>;

function step(
  href: string,
  go: Go,
  next: Pick<
    Step,
    "target" | "title" | "content" | "placement" | "skipScroll" | "isFixed"
  >,
): Step {
  return {
    skipBeacon: true,
    placement: "bottom",
    before: async () => {
      await go(href);
      if (typeof next.target === "string") {
        await waitForTourTarget(next.target);
      }
    },
    ...next,
  };
}

export function studentTourSteps(go: Go): Step[] {
  const profile = studentProfileHref();
  const rankings = studentRankingsHref();
  const catalog = projectsHref();

  return [
    step(profile, go, {
      target: '[data-tour="student-portal"]',
      title: "Your student portal",
      content:
        "Start here. Complete your profile first, then rank up to three faculty projects for matching.",
    }),
    step(profile, go, {
      target: '[data-tour="student-profile"]',
      title: "Keep your profile current",
      content:
        "Faculty and the matching process use this profile. Edit it whenever your details change.",
    }),
    step(profile, go, {
      target: '[data-tour="student-interests"]',
      title: "Research interests",
      content:
        "Choose topics and weekly availability so projects that fit you are easier to spot.",
    }),
    step(rankings, go, {
      target: '[data-tour="student-rankings"]',
      title: "Your ranked projects",
      content:
        "Your 1st, 2nd, and 3rd choices appear here. Matching is based on these rankings.",
    }),
    step(catalog, go, {
      target: '[data-tour="project-filters"]',
      title: "Browse open projects",
      content:
        "Filter by research area or activity type to find a listing you want to join.",
      placement: "right",
    }),
    step(catalog, go, {
      target: '[data-tour="project-catalog"]',
      title: "Express interest",
      content:
        "Open a listing and choose Express interest to rank it as one of your top 3.",
    }),
    step(catalog, go, {
      target: '[data-tour="account-tools"]',
      title: "Notifications and account",
      content:
        "The bell shows matching updates. Profile opens this portal again whenever you need it.",
      placement: "bottom",
      skipScroll: true,
      isFixed: true,
    }),
  ];
}

export function facultyTourSteps(go: Go, userId: string): Step[] {
  const list = myProjectsHref(userId);
  const create = newProjectHref(userId);

  return [
    step(list, go, {
      target: '[data-tour="faculty-projects"]',
      title: "Your project listings",
      content:
        "Posted research opportunities live here, with status, seats, and actions for each one.",
    }),
    step(list, go, {
      target: '[data-tour="faculty-new-project"]',
      title: "Post a project",
      content:
        "Create a new listing so undergraduates can find your work and rank it.",
      placement: "bottom",
    }),
    step(create, go, {
      target: '[data-tour="faculty-project-details"]',
      title: "Describe the research",
      content:
        "Add the title, research areas, and what volunteers will do on the project.",
    }),
    step(create, go, {
      target: '[data-tour="faculty-volunteers"]',
      title: "Seats and student ranking",
      content:
        "Set how many students you need. After they rank this listing, open it from My projects and rank them as your 1st, 2nd, or 3rd choice.",
    }),
    step(create, go, {
      target: '[data-tour="account-tools"]',
      title: "Stay notified",
      content:
        "The bell tells you when students rank your work and when matching fills seats.",
      placement: "bottom",
      skipScroll: true,
      isFixed: true,
    }),
  ];
}
