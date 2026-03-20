import { useState } from "react";
import { useShowErrorToast } from "../utils/toasts";
import { toast } from "sonner";
import { Grid, Heading, Section, Dialog } from "@radix-ui/themes";
import { ReservationCard } from "./ReservationCard";
import { bookRoom, parseApiError, NewReservation, Reservation, useGetRooms } from "./api";
import { LoadingCard } from "../components/LoadingCard";
import { BookingDetailsModal } from "./BookingDetailsModal";
import { BookingConfirmationModal } from "./BookingConfirmationModal";

const RESPONSIVE_GRID_COLS: React.ComponentProps<typeof Grid>["columns"] = {
  sm: "1",
  md: "2",
  lg: "4",
};

export function ReservationPage() {
  const { isLoading, data: rooms } = useGetRooms();
  const [selectedRoomNumber, setSelectedRoomNumber] = useState("");

  const formattedRoomNumber = String(selectedRoomNumber).padStart(3, "0");

  const [confirmedReservation, setConfirmedReservation] = useState<Reservation | null>(null);
  const showError = useShowErrorToast();

  function onClose() {
    setSelectedRoomNumber("");
  }

  async function onSubmit(booking: NewReservation) {
    const toastId = toast.loading("Processing booking...");
    try {
      const reservation = await bookRoom(booking);
      toast.dismiss(toastId);
      onClose();
      setConfirmedReservation(reservation);
    } catch (error) {
      toast.dismiss(toastId);
      const errors = await parseApiError(error);
      showError(errors);
    }
  }

  const createClickHandler = (roomNumber: string) => () => {
    setSelectedRoomNumber(roomNumber);
  };

  return (
    <Section size="2" px="2">
      <Heading size="8" as="h1" color="mint">
        Rooms
      </Heading>

      <Grid columns={RESPONSIVE_GRID_COLS} gap="4" px="4" mt="8">
        <Dialog.Root>
          {isLoading && <LoadingCard />}
          {rooms?.map((room) => (
            <ReservationCard
              key={room.number}
              imgSrc="/bed.png"
              roomNumber={room.number}
              onClick={createClickHandler(room.number)}
            />
          ))}

          <BookingDetailsModal
            roomNumber={formattedRoomNumber}
            onSubmit={onSubmit}
          />
        </Dialog.Root>
      </Grid>

      {confirmedReservation && (
        <BookingConfirmationModal
          reservation={confirmedReservation}
          onClose={() => setConfirmedReservation(null)}
        />
      )}
    </Section>
  );
}
