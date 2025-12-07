import { DefaultSession } from "next-auth";

declare module "next-auth" {
  interface Session {
    accessToken: string;
    user: {
      id: string;
      role: "Provider" | "Client" | "TeamMember" | "PlatformAdmin";
      tenantId: string;
      name: string;
    } & DefaultSession["user"];
  }

  interface User {
    token: string;
    id: string;
    role: string;
    tenantId: string;
    name: string;
  }
}

declare module "next-auth/jwt" {
  interface JWT {
    accessToken: string;
    userId: string;
    role: string;
    tenantId: string;
  }
}
