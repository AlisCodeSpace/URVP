import { apiFetch } from "@/lib/api";
import {
  introHeadline,
  introKeyFacts,
  introParagraphs,
} from "@/lib/home-content";

export type HomeIntroDto = {
  headline: string;
  description: string;
  keyPoints: string[];
  updatedAt: string;
};

export type UpdateHomeIntroPayload = {
  headline: string;
  description: string;
  keyPoints: string[];
};

export const HOME_INTRO_MAX_KEY_POINTS = 5;
export const HOME_INTRO_HEADLINE_MAX_LENGTH = 200;
export const HOME_INTRO_DESCRIPTION_MAX_LENGTH = 8000;
export const HOME_INTRO_KEY_POINT_MAX_LENGTH = 400;

export const defaultHomeIntro: HomeIntroDto = {
  headline: introHeadline,
  description: introParagraphs.join("\n\n"),
  keyPoints: [...introKeyFacts],
  updatedAt: "",
};

export function splitIntroDescription(description: string): string[] {
  return description
    .split(/\n\s*\n/)
    .map((paragraph) => paragraph.trim())
    .filter(Boolean);
}

export async function getHomeIntro(): Promise<HomeIntroDto> {
  return apiFetch<HomeIntroDto>("/api/home-intro");
}

export async function updateHomeIntro(
  payload: UpdateHomeIntroPayload,
): Promise<HomeIntroDto> {
  return apiFetch<HomeIntroDto>("/api/home-intro", {
    method: "PUT",
    body: JSON.stringify(payload),
  });
}

export async function loadPublicHomeIntro(): Promise<HomeIntroDto> {
  try {
    const intro = await getHomeIntro();
    const headline = intro.headline.trim();
    const description = intro.description.trim();
    const keyPoints = intro.keyPoints
      .map((point) => point.trim())
      .filter(Boolean)
      .slice(0, HOME_INTRO_MAX_KEY_POINTS);
    return {
      headline: headline || defaultHomeIntro.headline,
      description: description || defaultHomeIntro.description,
      keyPoints,
      updatedAt: intro.updatedAt,
    };
  } catch {
    return defaultHomeIntro;
  }
}
