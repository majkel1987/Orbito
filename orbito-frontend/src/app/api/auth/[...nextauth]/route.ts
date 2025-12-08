import NextAuth, { type User, type Session } from "next-auth";
import { type JWT } from "next-auth/jwt";
import CredentialsProvider from "next-auth/providers/credentials";
import apiClient from "@/core/api/client";

const authOptions = {
  secret: process.env.NEXTAUTH_SECRET,
  providers: [
    CredentialsProvider({
      name: "Credentials",
      credentials: {
        email: { label: "Email", type: "email" },
        password: { label: "Password", type: "password" },
      },
      async authorize(credentials) {
        if (!credentials?.email || !credentials?.password) {
          return null;
        }

        try {
          // Backend API call - Result<T> interceptor handles the response
          const response = await apiClient.post<{
            token: string;
            user: {
              id: string;
              email: string;
              firstName: string;
              lastName: string;
              tenantId: string | null;
              roles: string[];
            };
          }>("/api/Account/login", {
            email: credentials.email,
            password: credentials.password,
          });

          // Result<T> interceptor returns response.data directly (value from backend)
          const { token, user } = response.data;

          console.log("Backend response:", { token, user });

          // Map backend response to NextAuth user format
          const fullName = `${user.firstName || ""} ${user.lastName || ""}`.trim() || user.email || "User";
          const role = (user.roles?.[0] as
            | "Provider"
            | "Client"
            | "TeamMember"
            | "PlatformAdmin") || "Client";

          const userObject = {
            id: user.id,
            email: user.email || "",
            name: fullName,
            role: role,
            tenantId: user.tenantId || null,
            token,
          };

          console.log("Mapped user object:", userObject);

          // Return user object with token
          return userObject;
        } catch (error) {
          console.error("Authentication failed:", error);
          if (error instanceof Error) {
            console.error("Error message:", error.message);
            console.error("Error stack:", error.stack);
          }
          return null;
        }
      },
    }),
  ],
  callbacks: {
    async jwt({ token, user }: { token: JWT; user: User | undefined }) {
      // On sign in, user object is available
      if (user) {
        token.accessToken = user.token;
        token.userId = user.id;
        token.role = user.role;
        token.tenantId = user.tenantId;
        token.name = user.name;
      }
      return token;
    },
    async session({ session, token }: { session: Session; token: JWT }) {
      // Attach data from JWT token to session
      session.accessToken = token.accessToken as string;
      session.user.id = token.userId as string;
      session.user.role = token.role as "Provider" | "Client" | "TeamMember" | "PlatformAdmin";
      session.user.tenantId = token.tenantId as string | null;
      session.user.name = token.name as string;
      return session;
    },
  },
  pages: {
    signIn: "/login",
    error: "/auth/error",
  },
  session: {
    strategy: "jwt" as const,
  },
};

const { handlers } = NextAuth(authOptions);
export const { GET, POST } = handlers;
