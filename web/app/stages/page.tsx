import type { Metadata } from "next";
import { Container } from "@/components/ui/container/container";
import { Page } from "@/components/ui/page/page";
import { PageTitle } from "@/components/ui/page-title/page-title";
import { Stack } from "@/components/ui/stack/stack";
import { StageForm } from "./components/stage-form";
import { StageList } from "./components/stage-list";

export const metadata: Metadata = {
  title: "Stages | alpinarc",
};

export default function StagesPage() {
  return (
    <Page>
      <Container>
        <Stack>
          <PageTitle>Stages</PageTitle>
          <StageList />
          <StageForm />
        </Stack>
      </Container>
    </Page>
  );
}
