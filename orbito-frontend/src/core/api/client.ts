import axios, { AxiosError, AxiosRequestConfig } from "axios";

// Backend Result<T> type definition
interface Result<T> {
  isSuccess: boolean;
  value?: T;
  error?: string;
  errors?: string[];
}

// Create axios instance with base configuration
// Note: baseURL should NOT include /api because generated endpoints already include it
const axiosInstance = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_URL || "http://localhost:5211",
  headers: {
    "Content-Type": "application/json",
  },
});

// Auth interceptor is now handled by AuthInterceptorProvider component

// Response interceptor: Handle Result<T> and map errors
axiosInstance.interceptors.response.use(
  (response) => {
    const data = response.data;

    // Check if response is in Result<T> format
    if (data && typeof data === "object" && "isSuccess" in data) {
      const result = data as Result<unknown>;

      if (result.isSuccess) {
        let unwrappedData = result.value;

        // Fix backend inconsistency: map 'clients' to 'items' for ClientDtoPaginatedList
        if (
          unwrappedData &&
          typeof unwrappedData === "object" &&
          "clients" in unwrappedData &&
          Array.isArray((unwrappedData as { clients: unknown }).clients)
        ) {
          const { clients, ...rest } = unwrappedData as {
            clients: unknown[];
            [key: string]: unknown;
          };
          unwrappedData = { ...rest, items: clients };
        }

        // Success: return unwrapped value
        return { ...response, data: unwrappedData };
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
      const data = error.response.data;

      // For 400 Bad Request, check if response is Result<T> and extract error message
      if (status === 400) {
        if (data && typeof data === "object" && "isSuccess" in data) {
          const result = data as Result<unknown>;
          if (!result.isSuccess) {
            const errorMessage =
              result.error ||
              (result.errors && result.errors.length > 0
                ? result.errors.join(", ")
                : "Bad request");
            throw new Error(errorMessage);
          }
        }
        // If not Result<T> format, preserve the original error
        return Promise.reject(error);
      }

      // For 401 Unauthorized, preserve the original error
      // so that components/handlers can access error details
      if (status === 401) {
        return Promise.reject(error);
      }

      switch (status) {
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
