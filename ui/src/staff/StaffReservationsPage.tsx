import { useState } from "react";
import { Box, Card, Flex, Heading, Section, Select, Table, Text, TextField } from "@radix-ui/themes";
import { useNavigate } from "@tanstack/react-router";
import { useAuth } from "../utils/auth";
import { ReservationFilters, useGetStaffReservations } from "./api";
import { useGetRooms } from "../reservations/api";
import { LoadingCard } from "../components/LoadingCard";

function todayStr() {
  return new Date().toISOString().split("T")[0];
}

export function StaffReservationsPage() {
  const { token, isAuthenticated } = useAuth();
  const navigate = useNavigate();

  const [filters, setFilters] = useState<ReservationFilters>({
    from: todayStr(),
  });

  const { data: reservations, isLoading } = useGetStaffReservations(token, filters);
  const { data: rooms } = useGetRooms();

  if (!isAuthenticated) {
    navigate({ to: "/" });
    return null;
  }

  function updateFilter(key: keyof ReservationFilters, value: string) {
    setFilters((prev) => ({ ...prev, [key]: value || undefined }));
  }

  return (
    <Section size="2" px="2">
      <Heading size="8" as="h1" color="mint" mb="6">
        Reservations
      </Heading>

      <Card size="2" mb="4" variant="surface">
        <Flex gap="4" wrap="wrap" align="end">
          <Box style={{ flex: 1, minWidth: 150 }}>
            <Text as="label" size="2" weight="medium" color="gray" mb="1">From</Text>
            <TextField.Root
              type="date"
              value={filters.from ?? ""}
              onChange={(e) => updateFilter("from", e.target.value)}
              size="2"
            />
          </Box>
          <Box style={{ flex: 1, minWidth: 150 }}>
            <Text as="label" size="2" weight="medium" color="gray" mb="1">To</Text>
            <TextField.Root
              type="date"
              value={filters.to ?? ""}
              onChange={(e) => updateFilter("to", e.target.value)}
              size="2"
            />
          </Box>
          <Box style={{ flex: 1, minWidth: 120 }}>
            <Text as="label" size="2" weight="medium" color="gray" mb="1">Room</Text>
            <Select.Root
              value={filters.roomNumber ?? "all"}
              onValueChange={(v) => updateFilter("roomNumber", v === "all" ? "" : v)}
              size="2"
            >
              <Select.Trigger style={{ width: "100%" }} />
              <Select.Content>
                <Select.Item value="all">All rooms</Select.Item>
                {rooms?.map((room) => (
                  <Select.Item key={room.number} value={room.number}>
                    #{room.number}
                  </Select.Item>
                ))}
              </Select.Content>
            </Select.Root>
          </Box>
          <Box style={{ flex: 2, minWidth: 200 }}>
            <Text as="label" size="2" weight="medium" color="gray" mb="1">Guest Email</Text>
            <TextField.Root
              placeholder="e.g. guest@email.com"
              value={filters.guestEmail ?? ""}
              onChange={(e) => updateFilter("guestEmail", e.target.value)}
              size="2"
            />
          </Box>
        </Flex>
      </Card>

      {isLoading && <LoadingCard />}

      {reservations && reservations.length === 0 && (
        <Box py="4">No reservations found.</Box>
      )}

      {reservations && reservations.length > 0 && (
        <Table.Root variant="surface">
          <Table.Header>
            <Table.Row>
              <Table.ColumnHeaderCell>Room</Table.ColumnHeaderCell>
              <Table.ColumnHeaderCell>Guest Email</Table.ColumnHeaderCell>
              <Table.ColumnHeaderCell>Check-in</Table.ColumnHeaderCell>
              <Table.ColumnHeaderCell>Check-out</Table.ColumnHeaderCell>
            </Table.Row>
          </Table.Header>
          <Table.Body>
            {reservations.map((r) => (
              <Table.Row key={r.id}>
                <Table.Cell>#{r.roomNumber}</Table.Cell>
                <Table.Cell>{r.guestEmail}</Table.Cell>
                <Table.Cell>{new Date(r.start).toLocaleDateString()}</Table.Cell>
                <Table.Cell>{new Date(r.end).toLocaleDateString()}</Table.Cell>
              </Table.Row>
            ))}
          </Table.Body>
        </Table.Root>
      )}
    </Section>
  );
}
