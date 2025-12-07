import axios, { AxiosError, AxiosRequestConfig } from "axios";

// Backend Result<T> type definition
interface Result<T> {
  isSuccess: boolean;
  value?: T;
  error?: string;
  errors?: string[];
}

// Create axios instance with base configuration
const axiosInstance = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000/api",
  headers: {
    "Content-Type": "application/json",
  },
});

// TODO: Request interceptor for auth will be added in Block 1.1 (NextAuth setup)
// Will add Bearer token from NextAuth session when available

// Response interceptor: Handle Result<T> and map errors
axiosInstance.interceptors.response.use(
  (response) => {
    const data = response.data;

    // Check if response is in Result<T> format
    if (data && typeof data === "object" && "isSuccess" in data) {
      const result = data as Result<unknown>;

      if (result.isSuccess) {
        // Success: return unwrapped value
        return { ...response, data: result.value };
      } else {
        // Failure: throw error with backend message
        const errorMessage =
          result.error ||
          (result.errors && result.errors.length > 0
            ? result.errors.join(", ")
            : "Unknown error occurred");

        throw new Error(errorMessage);
      }
    }

    // If not Result<T> format, return as-is
    return response;
  },
  (error: AxiosError) => {
    // Map HTTP errors to user-friendly messages
    if (error.response) {
      const status = error.response.status;

      switch (status) {
        case 401:
          throw new Error("Unauthorized. Please log in again.");
        case 403:
          throw new Error(
            "Forbidden. You don't have permission to access this resource."
          );
        case 404:
          throw new Error("Resource not found.");
        case 500:
          throw new Error("Server error. Please try again later.");
        default:
          throw new Error(
            error.message || "An unexpected error occurred."
          );
      }
    }

    // Network errors
    if (error.request) {
      throw new Error(
        "Network error. Please check your connection and try again."
      );
    }

    // Other errors
    throw error;
  }
);

// Custom instance for Orval
export const customInstance = <T>(
  config: AxiosRequestConfig,
  options?: AxiosRequestConfig
): Promise<T> => {
  const source = axios.CancelToken.source();
  const promise = axiosInstance({
    ...config,
    ...options,
    cancelToken: source.token,
  }).then(({ data }) => data);

  // @ts-expect-error: Adding cancel method to promise
  promise.cancel = () => {
    source.cancel("Query was cancelled");
  };

  return promise;
};

export default axiosInstance;
