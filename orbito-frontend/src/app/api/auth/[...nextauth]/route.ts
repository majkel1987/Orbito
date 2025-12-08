import NextAuth, { type User, type Session } from "next-auth";
import { type JWT } from "next-auth/jwt";
import CredentialsProvider from "next-auth/providers/credentials";
import apiClient from "@/core/api/client";

const authOptions = {
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
              name: string;
              role: "Provider" | "Client" | "TeamMember" | "PlatformAdmin";
              tenantId: string;
            };
          }>("/auth/login", {
            email: credentials.email,
            password: credentials.password,
          });

          // Result<T> interceptor returns response.data directly (value from backend)
          const { token, user } = response.data;

          // Return user object with token
          return {
            id: user.id,
            email: user.email,
            name: user.name,
            role: user.role,
            tenantId: user.tenantId,
            token,
          };
        } catch (error) {
          console.error("Authentication failed:", error);
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
      session.user.tenantId = token.tenantId as string;
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
