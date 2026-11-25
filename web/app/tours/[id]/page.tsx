import type { Metadata } from "next";
import { Suspense } from "react";
import { Container } from "@/components/ui/container/container";
import { Page } from "@/components/ui/page/page";
import { LoadingState } from "@/components/ui/state/loading-state";
import { TourDetails } from "./components/tour-details";

export const metadata: Metadata = {
  title: "Tour | alpinarc",
};

export default function TourPage({ params }: PageProps<"/tours/[id]">) {
  // The id is only known at request time, so the rest of the page is
  // prerendered and the details stream in behind the fallback.
  return (
    <Page>
      <Container>
        <Suspense fallback={<LoadingState entityName="tour" />}>
          <TourDetails params={params} />
        </Suspense>
      </Container>
    </Page>
  );
}
