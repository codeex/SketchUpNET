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
#include <slapi/model/component_instance.h>
#include <slapi/model/component_definition.h>
#include <msclr/marshal.h>
#include <vector>
#include "transform.h"
#include "component.h"


#pragma once

using namespace System;
using namespace System::Collections;
using namespace System::Collections::Generic;

namespace SketchUpNET
{
	public ref class Instance
	{
	public:
		System::String^ Name;
		Transform^ Transformation;
		Component^ Parent;
		System::String^ Guid;

		Instance(System::String^ name, System::String^ guid, Component^ parent, Transform^ transformation)
		{
			this->Name = name;
			this->Transformation = transformation;
			this->Parent = parent;
			this->Guid = guid;
		};

		Instance(){};
	internal:
		static Instance^ FromSU(SUComponentInstanceRef comp, Dictionary<String^, Component^>^ components)
		{
			SUStringRef name = SU_INVALID;
			SUStringCreate(&name);
			SUComponentInstanceGetName(comp, &name);

			SUComponentDefinitionRef definition = SU_INVALID;
			SUComponentInstanceGetDefinition(comp, &definition);

			SUStringRef instanceguid = SU_INVALID;
			SUStringCreate(&instanceguid);
			SUComponentInstanceGetGuid(comp, &instanceguid);




			SUStringRef guid = SU_INVALID;
			SUStringCreate(&guid);
			SUComponentDefinitionGetGuid(definition, &guid);
			System::String^ guidstring = SketchUpNET::Utilities::GetString(guid);

			Component^ parent = components[guidstring];


			SUTransformation transform = SU_INVALID;
			SUComponentInstanceGetTransform(comp, &transform);


			Instance^ v = gcnew Instance(SketchUpNET::Utilities::GetString(name), SketchUpNET::Utilities::GetString(instanceguid), parent, Transform::FromSU(transform));

			return v;
		};
		static List<Instance^>^ GetEntityInstances(SUEntitiesRef entities, Dictionary<String^, Component^>^ components)
		{
			List<Instance^>^ instancelist = gcnew List<Instance^>();

			//Get All Component Instances

			size_t instanceCount = 0;
			SUEntitiesGetNumInstances(entities, &instanceCount);

			if (instanceCount > 0) {
				std::vector<SUComponentInstanceRef> instances(instanceCount);
				SUEntitiesGetInstances(entities, instanceCount, &instances[0], &instanceCount);

				for (size_t i = 0; i < instanceCount; i++) {
					Instance^ inst = Instance::FromSU(instances[i], components);
					instancelist->Add(inst);
				}

			}

			return instancelist;
		}
	};


}