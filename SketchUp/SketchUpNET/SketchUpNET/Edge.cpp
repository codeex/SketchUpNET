/*

SketchUpNET - a C++ Wrapper for the Trimble(R) SketchUp(R) C API
Copyright(C) 2015, Autor: Maximilian Thumfart

Permission is hereby granted, free of charge, to any person obtaining a copy of this software
and associated documentation files (the "Software"), to deal in the Software without restriction,
including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

*/

#include <slapi/slapi.h>
#include <slapi/geometry.h>
#include <slapi/initialize.h>
#include <slapi/unicodestring.h>
#include <slapi/model/model.h>
#include <slapi/model/entities.h>
#include <slapi/model/face.h>
#include <slapi/model/edge.h>
#include <slapi/model/vertex.h>
#include <slapi/model/curve.h>
#include <msclr/marshal.h>
#include <vector>
#include "vertex.h"

#pragma once

using namespace System;
using namespace System::Collections;
using namespace System::Collections::Generic;

namespace SketchUpNET
{
	public ref class Edge
	{
	public:

		Vertex^ Start;
		Vertex^ End;

		Edge(Vertex ^ start, Vertex ^ end)
		{
			this->Start = start;
			this->End = end;
		};

		Edge(){};
	internal:
		static Edge^ FromSU(SUEdgeRef edge)
		{
			SUVertexRef startVertex = SU_INVALID;
			SUVertexRef endVertex = SU_INVALID;
			SUEdgeGetStartVertex(edge, &startVertex);
			SUEdgeGetEndVertex(edge, &endVertex);
			SUPoint3D start;
			SUPoint3D end;
			SUVertexGetPosition(startVertex, &start);
			SUVertexGetPosition(endVertex, &end);

			Edge^ v = gcnew Edge(Vertex::FromSU(start), Vertex::FromSU(end));

			return v;
		};

		SUEdgeRef ToSU()
		{
			SUEdgeRef edge = SU_INVALID;
			SUPoint3D start = this->Start->ToSU();
			SUPoint3D end = this->End->ToSU();
			SUEdgeCreate(&edge,&start,&end);
			return edge;
		}

		static SUEdgeRef* ListToSU(List<Edge^>^ list)
		{
			size_t size = list->Count;
			SUEdgeRef * result = (SUEdgeRef *)malloc(*&size * sizeof(SUEdgeRef));
			for (int i = 0; i < size; i++)
			{
				result[i] = list[i]->ToSU();
			}
			return result;
		}

		static List<Edge^>^ GetEntityEdges(SUEntitiesRef entities)
		{
			List<Edge^>^ edges = gcnew List<Edge^>();

			// Get Edges
			size_t edgeCount = 0;
			SUEntitiesGetNumEdges(entities, false, &edgeCount);

			if (edgeCount > 0)
			{
				std::vector<SUEdgeRef> edgevector(edgeCount);
				SUEntitiesGetEdges(entities, false, edgeCount, &edgevector[0], &edgeCount);


				for (size_t i = 0; i < edgeCount; i++) {
					Edge^ edge = Edge::FromSU(edgevector[i]);
					edges->Add(edge);
				}
			}

			return edges;
		}


	};




}