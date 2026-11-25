import type { Metadata } from "next";
import { Container } from "@/components/container/container";
import { Page } from "@/components/page/page";
import { PageTitle } from "@/components/page-title/page-title";
import { Stack } from "@/components/stack/stack";
import { LodgeForm } from "./components/lodge-form";
import { LodgeList } from "./components/lodge-list";

export const metadata: Metadata = {
  title: "Lodges | alpinarc",
};

export default function LodgesPage() {
  return (
    <Page>
      <Container>
        <Stack>
          <PageTitle>Lodges</PageTitle>
          <LodgeForm />
          <LodgeList />
        </Stack>
      </Container>
    </Page>
  );
}
