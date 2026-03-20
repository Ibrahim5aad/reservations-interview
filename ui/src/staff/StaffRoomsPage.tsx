import { useMemo, useState } from "react";
import { Badge, Box, Button, Card, Flex, Heading, Section, Select, Table, Text } from "@radix-ui/themes";
import { useNavigate } from "@tanstack/react-router";
import { useAuth } from "../utils/auth";
import { useGetRooms } from "../reservations/api";
import { RoomStateLabels } from "./api";
import { ImportRoomsDialog } from "./ImportRoomsDialog";
import { LoadingCard } from "../components/LoadingCard";

const PAGE_SIZE = 20;

const STATE_COLORS: Record<number, "green" | "orange" | "yellow"> = {
  0: "green",
  1: "orange",
  2: "yellow",
};

export function StaffRoomsPage() {
  const { isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const { data: rooms, isLoading } = useGetRooms();
  const [page, setPage] = useState(0);
  const [statusFilter, setStatusFilter] = useState("all");
  const [floorFilter, setFloorFilter] = useState("all");

  if (!isAuthenticated) {
    navigate({ to: "/" });
    return null;
  }

  const floors = useMemo(
    () => [...new Set(rooms?.map((r) => r.number[0]) ?? [])].sort(),
    [rooms]
  );

  const filteredRooms = useMemo(() => {
    let result = rooms ?? [];
    if (statusFilter !== "all") {
      result = result.filter((r) => r.state === Number(statusFilter));
    }
    if (floorFilter !== "all") {
      result = result.filter((r) => r.number[0] === floorFilter);
    }
    return result;
  }, [rooms, statusFilter, floorFilter]);

  const totalPages = useMemo(() => Math.ceil(filteredRooms.length / PAGE_SIZE), [filteredRooms]);
  const pagedRooms = useMemo(
    () => filteredRooms.slice(page * PAGE_SIZE, (page + 1) * PAGE_SIZE),
    [filteredRooms, page]
  );

  return (
    <Section size="1" px="2">
      <Flex justify="between" align="center" mb="6">
        <Heading size="8" as="h1" color="mint">
          Rooms
        </Heading>
        <ImportRoomsDialog>
          <Button color="mint" size="2">Import CSV</Button>
        </ImportRoomsDialog>
      </Flex>

      <Card size="2" mb="4" variant="surface">
        <Flex gap="4" align="end">
          <Box style={{ minWidth: 140 }}>
            <Text as="label" size="2" weight="medium" color="gray" mb="1">Status</Text>
            <Select.Root
              value={statusFilter}
              onValueChange={(v) => { setStatusFilter(v); setPage(0); }}
              size="2"
            >
              <Select.Trigger style={{ width: "100%" }} />
              <Select.Content>
                <Select.Item value="all">All</Select.Item>
                <Select.Item value="0">Ready</Select.Item>
                <Select.Item value="1">Occupied</Select.Item>
                <Select.Item value="2">Dirty</Select.Item>
              </Select.Content>
            </Select.Root>
          </Box>
          <Box style={{ minWidth: 140 }}>
            <Text as="label" size="2" weight="medium" color="gray" mb="1">Floor</Text>
            <Select.Root
              value={floorFilter}
              onValueChange={(v) => { setFloorFilter(v); setPage(0); }}
              size="2"
            >
              <Select.Trigger style={{ width: "100%" }} />
              <Select.Content>
                <Select.Item value="all">All floors</Select.Item>
                {floors.map((f) => (
                  <Select.Item key={f} value={f}>Floor {f}</Select.Item>
                ))}
              </Select.Content>
            </Select.Root>
          </Box>
        </Flex>
      </Card>

      {isLoading && <LoadingCard />}

      {!isLoading && filteredRooms.length === 0 && (
        <Box py="4">No rooms found.</Box>
      )}

      {pagedRooms.length > 0 && (
        <>
          <Table.Root variant="surface">
            <Table.Header>
              <Table.Row>
                <Table.ColumnHeaderCell>Room Number</Table.ColumnHeaderCell>
                <Table.ColumnHeaderCell>Floor</Table.ColumnHeaderCell>
                <Table.ColumnHeaderCell>Status</Table.ColumnHeaderCell>
              </Table.Row>
            </Table.Header>
            <Table.Body>
              {pagedRooms.map((room) => (
                <Table.Row key={room.number}>
                  <Table.Cell>#{room.number}</Table.Cell>
                  <Table.Cell>Floor {room.number[0]}</Table.Cell>
                  <Table.Cell>
                    <Badge color={STATE_COLORS[room.state] ?? "gray"}>
                      {RoomStateLabels[room.state] ?? "Unknown"}
                    </Badge>
                  </Table.Cell>
                </Table.Row>
              ))}
            </Table.Body>
          </Table.Root>

          {totalPages > 1 && (
            <Flex justify="between" align="center" mt="3">
              <Text size="2" color="gray">
                Page {page + 1} of {totalPages} ({filteredRooms.length} rooms)
              </Text>
              <Flex gap="2">
                <Button
                  size="1"
                  variant="outline"
                  disabled={page === 0}
                  onClick={() => setPage((p) => p - 1)}
                >
                  Previous
                </Button>
                <Button
                  size="1"
                  variant="outline"
                  disabled={page >= totalPages - 1}
                  onClick={() => setPage((p) => p + 1)}
                >
                  Next
                </Button>
              </Flex>
            </Flex>
          )}
        </>
      )}
    </Section>
  );
}
