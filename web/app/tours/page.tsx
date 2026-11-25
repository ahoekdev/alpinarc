import type { Metadata } from "next";
import { Container } from "@/components/ui/container/container";
import { Page } from "@/components/ui/page/page";
import { PageTitle } from "@/components/ui/page-title/page-title";
import { Stack } from "@/components/ui/stack/stack";
import { TourForm } from "./components/tour-form";
import { TourList } from "./components/tour-list";

export const metadata: Metadata = {
  title: "Tours | alpinarc",
};

export default function ToursPage() {
  return (
    <Page>
      <Container>
        <Stack>
          <PageTitle>Tours</PageTitle>
          <TourList />
          <TourForm />
        </Stack>
      </Container>
    </Page>
  );
}
