import { NextRequest, NextResponse } from "next/server";

export const dynamic = "force-dynamic";
export const runtime = "nodejs";

const HOP_BY_HOP = new Set([
  "connection",
  "content-encoding",
  "content-length",
  "host",
  "keep-alive",
  "proxy-authenticate",
  "proxy-authorization",
  "te",
  "trailers",
  "transfer-encoding",
  "upgrade",
]);

function backendOrigin(): string {
  const raw =
    process.env.API_URL?.trim() ||
    process.env.NEXT_PUBLIC_API_URL?.trim() ||
    "";
  return raw.replace(/\/$/, "");
}

async function proxy(
  request: NextRequest,
  path: string[],
): Promise<Response> {
  const origin = backendOrigin();
  if (!/^https?:\/\//i.test(origin)) {
    return NextResponse.json(
      {
        success: false,
        message:
          "API_URL or NEXT_PUBLIC_API_URL must be set so the web app can reach the API.",
      },
      { status: 500 },
    );
  }

  const url = `${origin}/api/${path.join("/")}${request.nextUrl.search}`;
  const headers = new Headers();
  request.headers.forEach((value, key) => {
    if (!HOP_BY_HOP.has(key.toLowerCase())) {
      headers.set(key, value);
    }
  });

  const method = request.method;
  const hasBody = method !== "GET" && method !== "HEAD";

  const upstream = await fetch(url, {
    method,
    headers,
    body: hasBody ? await request.arrayBuffer() : undefined,
    redirect: "manual",
    cache: "no-store",
  });

  const responseHeaders = new Headers();
  upstream.headers.forEach((value, key) => {
    if (HOP_BY_HOP.has(key.toLowerCase()) || key.toLowerCase() === "set-cookie") {
      return;
    }
    responseHeaders.append(key, value);
  });

  const body = await upstream.arrayBuffer();
  const response = new NextResponse(body, {
    status: upstream.status,
    headers: responseHeaders,
  });

  for (const cookie of upstream.headers.getSetCookie()) {
    response.headers.append("set-cookie", cookie);
  }

  return response;
}

type RouteContext = { params: Promise<{ path: string[] }> };

async function handle(request: NextRequest, context: RouteContext) {
  const { path } = await context.params;
  return proxy(request, path);
}

export const GET = handle;
export const POST = handle;
export const PUT = handle;
export const PATCH = handle;
export const DELETE = handle;
export const HEAD = handle;
export const OPTIONS = handle;
