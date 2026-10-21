import type { INestApplication } from '@nestjs/common';
import {
  DocumentBuilder,
  getSchemaPath,
  type OpenAPIObject,
  type SchemaObject,
  SwaggerModule,
} from '@nestjs/swagger';
import { z } from 'zod';

// Zod schemas registered here are added to the OpenAPI document as named
// components, so generated clients get named types instead of inline ones
export const openApiRegistry = z.registry<{ id: string }>();

// Reference a registered schema in @ApiBody / @ApiOkResponse etc.
export function schemaRef(schema: z.ZodType) {
  const meta = openApiRegistry.get(schema);

  if (!meta) {
    throw new Error('Schema is not registered in openApiRegistry');
  }

  return { $ref: getSchemaPath(meta.id) };
}

export function createOpenApiDocument(app: INestApplication): OpenAPIObject {
  const config = new DocumentBuilder()
    .setTitle('Alpinarc API')
    .setVersion('0.0.1')
    .build();

  const document = SwaggerModule.createDocument(app, config, {
    // LodgesController.findAll -> lodgesFindAll
    operationIdFactory: (controllerKey, methodKey) =>
      controllerKey
        .replace(/Controller$/, '')
        .replace(/^./, (c) => c.toLowerCase()) +
      methodKey.replace(/^./, (c) => c.toUpperCase()),
  });

  // Zod's JSONSchema type is wider than Swagger's SchemaObject (e.g. it
  // allows boolean schemas), but the openapi-3.0 target never emits those
  const { schemas } = z.toJSONSchema(openApiRegistry, {
    target: 'openapi-3.0',
    uri: (id) => getSchemaPath(id),
  }) as { schemas: Record<string, SchemaObject> };

  // Zod sets `$id` from the uri option, which OpenAPI 3.0 doesn't allow
  for (const schema of Object.values(schemas)) {
    delete (schema as { $id?: string }).$id;
  }

  document.components = {
    ...document.components,
    schemas: { ...document.components?.schemas, ...schemas },
  };

  return document;
}
