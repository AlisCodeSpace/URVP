import { request as httpsRequest } from "node:https";
import { request as httpRequest } from "node:http";
import type { IncomingHttpHeaders } from "node:http";
import { URL } from "node:url";
import { NextRequest, NextResponse } from "next/server";

export const dynamic = "force-dynamic";
export const runtime = "nodejs";

function backendOrigin(): string {
  const raw =
    process.env.API_URL?.trim() ||
    process.env.NEXT_PUBLIC_API_URL?.trim() ||
    "";
  return raw.replace(/\/$/, "");
}

function readSetCookies(headers: IncomingHttpHeaders): string[] {
  const value = headers["set-cookie"];
  if (!value) {
    return [];
  }
  return Array.isArray(value) ? value : [value];
}

function rewriteCookie(header: string): string {
  const parts = header
    .split(";")
    .map((part) => part.trim())
    .filter(Boolean)
    .filter((part) => {
      const key = part.split("=")[0]?.trim().toLowerCase();
      return key !== "domain" && key !== "samesite" && key !== "secure";
    });

  parts.push("Secure", "SameSite=Lax");
  return parts.join("; ");
}

function parseSetCookie(header: string): {
  name: string;
  value: string;
  httpOnly: boolean;
  path: string;
  expires?: Date;
  maxAge?: number;
} | null {
  const segments = header.split(";").map((part) => part.trim());
  const first = segments.shift();
  if (!first) {
    return null;
  }

  const eq = first.indexOf("=");
  if (eq <= 0) {
    return null;
  }

  const parsed: {
    name: string;
    value: string;
    httpOnly: boolean;
    path: string;
    expires?: Date;
    maxAge?: number;
  } = {
    name: first.slice(0, eq),
    value: first.slice(eq + 1),
    httpOnly: false,
    path: "/",
  };

  for (const segment of segments) {
    const sep = segment.indexOf("=");
    const key = (sep === -1 ? segment : segment.slice(0, sep)).trim().toLowerCase();
    const attr = sep === -1 ? "" : segment.slice(sep + 1).trim();

    if (key === "path" && attr) parsed.path = attr;
    else if (key === "httponly") parsed.httpOnly = true;
    else if (key === "expires" && attr) {
      const expires = new Date(attr);
      if (!Number.isNaN(expires.getTime())) parsed.expires = expires;
    } else if (key === "max-age" && attr) {
      const maxAge = Number(attr);
      if (Number.isFinite(maxAge)) parsed.maxAge = maxAge;
    }
  }

  return parsed;
}

function requestOnce(
  url: string,
): Promise<{ status: number; headers: IncomingHttpHeaders }> {
  return new Promise((resolve, reject) => {
    const target = new URL(url);
    const send = target.protocol === "https:" ? httpsRequest : httpRequest;
    const req = send(
      {
        protocol: target.protocol,
        hostname: target.hostname,
        port: target.port || undefined,
        path: `${target.pathname}${target.search}`,
        method: "GET",
        headers: { host: target.host },
      },
      (res) => {
        res.resume();
        res.on("end", () =>
          resolve({
            status: res.statusCode ?? 502,
            headers: res.headers,
          }),
        );
      },
    );
    req.on("error", reject);
    req.end();
  });
}

export async function GET(request: NextRequest) {
  const origin = backendOrigin();
  const email = request.nextUrl.searchParams.get("email")?.trim() ?? "";
  const fallbackCallback = `${request.nextUrl.origin}/auth/callback`;
  const returnUrl =
    request.nextUrl.searchParams.get("returnUrl")?.trim() || fallbackCallback;

  if (!email) {
    return NextResponse.redirect(
      new URL("/sign-in?error=authentication_failed", request.nextUrl.origin),
    );
  }

  if (!/^https?:\/\//i.test(origin)) {
    return NextResponse.redirect(
      new URL("/sign-in?error=authentication_failed", request.nextUrl.origin),
    );
  }

  const upstreamUrl = new URL("/api/auth/dev/signin", origin);
  upstreamUrl.searchParams.set("email", email);
  upstreamUrl.searchParams.set("returnUrl", returnUrl);

  const upstream = await requestOnce(upstreamUrl.toString());
  const response = NextResponse.redirect(returnUrl, 302);

  for (const cookie of readSetCookies(upstream.headers)) {
    response.headers.append("set-cookie", rewriteCookie(cookie));
    const parsed = parseSetCookie(cookie);
    if (!parsed) {
      continue;
    }
    response.cookies.set(parsed.name, parsed.value, {
      httpOnly: parsed.httpOnly,
      secure: true,
      sameSite: "lax",
      path: parsed.path,
      expires: parsed.expires,
      maxAge: parsed.maxAge,
    });
  }

  if (upstream.status >= 400) {
    return NextResponse.redirect(
      new URL("/sign-in?error=authentication_failed", request.nextUrl.origin),
    );
  }

  return response;
}
