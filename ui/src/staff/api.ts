import { useQuery } from "@tanstack/react-query";
import { api } from "../utils/api-client";
import { z } from "zod";

export async function staffLogin(accessCode: string): Promise<string> {
  const response = await api
    .post("/api/staff/login", {
      headers: { "X-Staff-Code": accessCode },
    })
    .json<{ token: string }>();

  return response.token;
}

const StaffReservationSchema = z.object({
  id: z.string(),
  roomNumber: z.string(),
  guestEmail: z.string(),
  start: z.string(),
  end: z.string(),
});

export type StaffReservation = z.infer<typeof StaffReservationSchema>;

const StaffReservationListSchema = StaffReservationSchema.array();

export interface ReservationFilters {
  from?: string;
  to?: string;
  roomNumber?: string;
  guestEmail?: string;
}

export function useGetStaffReservations(token: string | null, filters: ReservationFilters) {
  const searchParams: Record<string, string> = {};
  if (filters.from) searchParams.from = filters.from;
  if (filters.to) searchParams.to = filters.to;
  if (filters.roomNumber) searchParams.roomNumber = filters.roomNumber;
  if (filters.guestEmail) searchParams.guestEmail = filters.guestEmail;

  return useQuery({
    queryKey: ["staff-reservations", token, filters],
    enabled: !!token,
    retry: false,
    refetchOnWindowFocus: false,
    queryFn: () =>
      api
        .get("/api/reservations", {
          searchParams,
          headers: { Authorization: `Bearer ${token}` },
        })
        .json()
        .then(StaffReservationListSchema.parseAsync),
  });
}
